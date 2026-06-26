const state = {
    view: 'users',
    users: [],
    roles: [],
    endpoints: [],
    pageIndex: 0,
    pageSize: 12,
    totalUsers: 0,
    userSearch: '',
    endpointSearch: ''
};

const rootPath = getRootPath();
const elements = {
    status: document.getElementById('status'),
    viewTitle: document.getElementById('view-title'),
    usersBody: document.getElementById('users-body'),
    rolesBody: document.getElementById('roles-body'),
    endpointsBody: document.getElementById('endpoints-body'),
    usersPage: document.getElementById('users-page'),
    userSearch: document.getElementById('user-search'),
    endpointSearch: document.getElementById('endpoint-search'),
    passwordDialog: document.getElementById('password-dialog'),
    generatedPassword: document.getElementById('generated-password')
};

document.addEventListener('DOMContentLoaded', init);

function init() {
    document.querySelectorAll('.nav-item').forEach(button => {
        button.addEventListener('click', () => switchView(button.dataset.view));
    });

    document.getElementById('refresh-button').addEventListener('click', refreshCurrentView);
    document.getElementById('logout-button').addEventListener('click', logout);
    document.getElementById('add-user-button').addEventListener('click', addUser);
    document.getElementById('add-role-button').addEventListener('click', addRole);
    document.getElementById('prev-users').addEventListener('click', () => changeUsersPage(-1));
    document.getElementById('next-users').addEventListener('click', () => changeUsersPage(1));
    document.getElementById('copy-password').addEventListener('click', copyGeneratedPassword);

    elements.userSearch.addEventListener('keydown', event => {
        if (event.key === 'Enter') {
            state.userSearch = elements.userSearch.value.trim();
            state.pageIndex = 0;
            loadUsers();
        }
    });

    elements.userSearch.addEventListener('search', () => {
        state.userSearch = elements.userSearch.value.trim();
        state.pageIndex = 0;
        loadUsers();
    });

    elements.endpointSearch.addEventListener('input', () => {
        state.endpointSearch = elements.endpointSearch.value.trim().toLowerCase();
        renderEndpoints();
    });

    refreshCurrentView();
}

function getRootPath() {
    const path = window.location.pathname;
    const lastSlash = path.lastIndexOf('/');
    return lastSlash >= 0 ? path.slice(0, lastSlash + 1) : '/';
}

async function api(path, options = {}) {
    setStatus('Loading');
    const headers = {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest',
        ...options.headers
    };

    const token = localStorage.getItem('X-MiniAuth-Token');
    if (token) {
        headers.Authorization = `Bearer ${token}`;
    }

    let response;
    let payload;
    try {
        response = await fetch(`${rootPath}${path}`, {
            credentials: 'include',
            ...options,
            headers
        });
        payload = await readJson(response);
    } catch (error) {
        setStatus(error.message || 'Request failed', 'error');
        throw error;
    }

    if (response.status === 401) {
        localStorage.removeItem('X-MiniAuth-Token');
        window.location.href = `${rootPath}login.html?ReturnUrl=${encodeURIComponent(window.location.href)}`;
        return null;
    }

    if (response.status === 403) {
        setStatus('Forbidden', 'error');
        throw new Error('Forbidden');
    }

    if (!response.ok || payload.ok === false || payload.code >= 400) {
        const message = payload.message || response.statusText || 'Request failed';
        setStatus(message, 'error');
        throw new Error(message);
    }

    setStatus('Ready', 'success');
    return Object.prototype.hasOwnProperty.call(payload, 'data') ? payload.data : payload;
}

async function readJson(response) {
    const text = await response.text();
    if (!text) {
        return {};
    }

    try {
        return JSON.parse(text);
    } catch {
        return { message: text };
    }
}

function setStatus(message, type) {
    elements.status.textContent = message || '';
    elements.status.className = type ? `status ${type}` : 'status';
}

function switchView(view) {
    state.view = view;
    document.querySelectorAll('.nav-item').forEach(button => {
        button.classList.toggle('active', button.dataset.view === view);
    });

    document.querySelectorAll('.view').forEach(section => {
        section.classList.toggle('active', section.id === `${view}-view`);
    });

    elements.viewTitle.textContent = view[0].toUpperCase() + view.slice(1);
    refreshCurrentView();
}

function refreshCurrentView() {
    if (state.view === 'users') {
        loadUsers();
        return;
    }

    if (state.view === 'roles') {
        loadRoles();
        return;
    }

    loadEndpoints();
}

async function loadRoles() {
    const roles = await api('api/getRoles');
    if (!roles) {
        return;
    }

    state.roles = roles;
    renderRoles();
}

async function loadUsers() {
    if (!state.roles.length) {
        await loadRoles();
    }

    const result = await api('api/getUsers', {
        method: 'POST',
        body: JSON.stringify({
            pageIndex: state.pageIndex,
            pageSize: state.pageSize,
            search: state.userSearch
        })
    });

    if (!result) {
        return;
    }

    state.users = result.users || [];
    state.totalUsers = result.totalItems || state.users.length;
    renderUsers();
}

async function loadEndpoints() {
    const endpoints = await api('api/getAllEndpoints');
    if (!endpoints) {
        return;
    }

    state.endpoints = endpoints;
    renderEndpoints();
}

function renderRoles() {
    elements.rolesBody.innerHTML = '';
    if (!state.roles.length) {
        elements.rolesBody.appendChild(emptyRow(4, 'No roles found'));
        return;
    }

    state.roles.forEach(role => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td><input data-field="Name" value="${escapeAttribute(role.Name || '')}"></td>
            <td><label class="inline-check"><input type="checkbox" data-field="Enable" ${role.Enable === false ? '' : 'checked'}> Enabled</label></td>
            <td><input data-field="Remark" value="${escapeAttribute(role.Remark || '')}"></td>
            <td><div class="row-actions"><button data-action="save">Save</button><button class="danger" data-action="delete">Delete</button></div></td>
        `;

        row.querySelector('[data-action="save"]').addEventListener('click', async () => {
            role.Name = value(row, 'Name');
            role.Enable = checked(row, 'Enable');
            role.Remark = value(row, 'Remark');
            await api('api/saveRole', { method: 'POST', body: JSON.stringify(role) });
            await loadRoles();
        });

        row.querySelector('[data-action="delete"]').addEventListener('click', async () => {
            if (!role.Id || !confirm(`Delete role ${role.Name || ''}?`)) {
                return;
            }
            await api('api/deleteRole', { method: 'POST', body: JSON.stringify({ Id: role.Id }) });
            await loadRoles();
        });

        elements.rolesBody.appendChild(row);
    });
}

function renderUsers() {
    elements.usersBody.innerHTML = '';
    if (!state.users.length) {
        elements.usersBody.appendChild(emptyRow(6, 'No users found'));
    }

    state.users.forEach(user => {
        if (!Array.isArray(user.Roles)) {
            user.Roles = [];
        }

        const row = document.createElement('tr');
        row.innerHTML = `
            <td><div class="field-stack"><input data-field="Username" placeholder="Username" value="${escapeAttribute(user.Username || '')}"><input data-field="Mail" placeholder="Email" value="${escapeAttribute(user.Mail || '')}"></div></td>
            <td><div class="check-list"></div></td>
            <td><div class="field-stack"><input data-field="PhoneNumber" placeholder="Phone" value="${escapeAttribute(user.PhoneNumber || '')}"><label class="inline-check"><input type="checkbox" data-field="EmailConfirmed" ${user.EmailConfirmed ? 'checked' : ''}> Email confirmed</label><label class="inline-check"><input type="checkbox" data-field="PhoneNumberConfirmed" ${user.PhoneNumberConfirmed ? 'checked' : ''}> Phone confirmed</label></div></td>
            <td><div class="field-stack"><input data-field="First_name" placeholder="First name" value="${escapeAttribute(user.First_name || '')}"><input data-field="Last_name" placeholder="Last name" value="${escapeAttribute(user.Last_name || '')}"><input data-field="Emp_no" placeholder="Employee number" value="${escapeAttribute(user.Emp_no || '')}"></div></td>
            <td><div class="field-stack"><label class="inline-check"><input type="checkbox" data-field="Enable" ${user.Enable === false ? '' : 'checked'}> Enabled</label><label class="inline-check"><input type="checkbox" data-field="TwoFactorEnabled" ${user.TwoFactorEnabled ? 'checked' : ''}> Two factor</label><label class="inline-check"><input type="checkbox" data-field="LockoutEnabled" ${user.LockoutEnabled ? 'checked' : ''}> Lockout</label><input type="datetime-local" data-field="LockoutEnd" value="${escapeAttribute(user.LockoutEnd || '')}"></div></td>
            <td><div class="row-actions"><button data-action="save">Save</button><button class="secondary" data-action="reset">Reset</button><button class="danger" data-action="delete">Delete</button></div></td>
        `;

        const roleList = row.querySelector('.check-list');
        state.roles.forEach(role => {
            const label = document.createElement('label');
            label.className = 'check-item';
            label.innerHTML = `<input type="checkbox" value="${escapeAttribute(role.Id)}" ${user.Roles.includes(role.Id) ? 'checked' : ''}> ${escapeHtml(role.Name || role.Id)}`;
            roleList.appendChild(label);
        });

        row.querySelector('[data-action="save"]').addEventListener('click', async () => {
            collectUser(row, user);
            const result = await api('api/saveUser', { method: 'POST', body: JSON.stringify(user) });
            if (result && result.newPassword) {
                showGeneratedPassword(result.newPassword);
            }
            await loadUsers();
        });

        row.querySelector('[data-action="reset"]').addEventListener('click', async () => {
            if (!user.Id || !confirm(`Reset password for ${user.Username || 'this user'}?`)) {
                return;
            }
            const result = await api('api/resetPassword', { method: 'POST', body: JSON.stringify({ ...user, Password: '' }) });
            if (result && result.newPassword) {
                showGeneratedPassword(result.newPassword);
            }
        });

        row.querySelector('[data-action="delete"]').addEventListener('click', async () => {
            if (!user.Id || !confirm(`Delete user ${user.Username || ''}?`)) {
                return;
            }
            await api('api/deleteUser', { method: 'POST', body: JSON.stringify({ Id: user.Id }) });
            await loadUsers();
        });

        elements.usersBody.appendChild(row);
    });

    const totalPages = Math.max(1, Math.ceil(state.totalUsers / state.pageSize));
    elements.usersPage.textContent = `Page ${Math.min(state.pageIndex + 1, totalPages)} of ${totalPages}`;
    document.getElementById('prev-users').disabled = state.pageIndex <= 0;
    document.getElementById('next-users').disabled = state.pageIndex >= totalPages - 1;
}

function renderEndpoints() {
    elements.endpointsBody.innerHTML = '';
    const endpoints = state.endpoints.filter(endpoint => {
        const search = state.endpointSearch;
        if (!search) {
            return true;
        }

        return `${endpoint.Name || ''} ${endpoint.Route || ''}`.toLowerCase().includes(search);
    });

    if (!endpoints.length) {
        elements.endpointsBody.appendChild(emptyRow(4, 'No endpoints found'));
        return;
    }

    endpoints.forEach(endpoint => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${escapeHtml(endpoint.Name || endpoint.Id || '')}</td>
            <td><code>${escapeHtml(endpoint.Route || '')}</code></td>
            <td><div class="badge-list">${(endpoint.Methods || []).map(method => `<span class="badge">${escapeHtml(method)}</span>`).join('') || '<span class="badge warning">Any</span>'}</div></td>
            <td>${endpoint.RedirectToLoginPage ? 'Login page' : '401'}</td>
        `;
        elements.endpointsBody.appendChild(row);
    });
}

function collectUser(row, user) {
    user.Username = value(row, 'Username');
    user.Mail = value(row, 'Mail');
    user.PhoneNumber = value(row, 'PhoneNumber');
    user.First_name = value(row, 'First_name');
    user.Last_name = value(row, 'Last_name');
    user.Emp_no = value(row, 'Emp_no');
    user.Enable = checked(row, 'Enable');
    user.EmailConfirmed = checked(row, 'EmailConfirmed');
    user.PhoneNumberConfirmed = checked(row, 'PhoneNumberConfirmed');
    user.TwoFactorEnabled = checked(row, 'TwoFactorEnabled');
    user.LockoutEnabled = checked(row, 'LockoutEnabled');
    user.LockoutEnd = value(row, 'LockoutEnd') || null;
    user.Roles = Array.from(row.querySelectorAll('.check-list input:checked')).map(input => input.value);
}

function addRole() {
    state.roles.unshift({ Id: null, Name: '', Enable: true, Remark: '' });
    renderRoles();
}

function addUser() {
    state.users.unshift({
        Id: null,
        Username: '',
        Mail: '',
        PhoneNumber: '',
        First_name: '',
        Last_name: '',
        Emp_no: '',
        Enable: true,
        EmailConfirmed: false,
        PhoneNumberConfirmed: false,
        TwoFactorEnabled: false,
        LockoutEnabled: false,
        LockoutEnd: null,
        Roles: []
    });
    renderUsers();
}

function changeUsersPage(direction) {
    const totalPages = Math.max(1, Math.ceil(state.totalUsers / state.pageSize));
    const nextPage = Math.min(totalPages - 1, Math.max(0, state.pageIndex + direction));
    if (nextPage !== state.pageIndex) {
        state.pageIndex = nextPage;
        loadUsers();
    }
}

async function logout() {
    localStorage.removeItem('X-MiniAuth-Token');
    window.location.href = `${rootPath}logout`;
}

function value(row, field) {
    const input = row.querySelector(`[data-field="${field}"]`);
    return input ? input.value : '';
}

function checked(row, field) {
    const input = row.querySelector(`[data-field="${field}"]`);
    return input ? input.checked : false;
}

function emptyRow(colspan, message) {
    const row = document.createElement('tr');
    row.innerHTML = `<td colspan="${colspan}" class="empty">${escapeHtml(message)}</td>`;
    return row;
}

function showGeneratedPassword(password) {
    elements.generatedPassword.value = password;
    if (typeof elements.passwordDialog.showModal === 'function') {
        elements.passwordDialog.showModal();
    } else {
        alert(`Generated password: ${password}`);
    }
}

async function copyGeneratedPassword() {
    const password = elements.generatedPassword.value;
    if (!password) {
        return;
    }

    if (navigator.clipboard) {
        await navigator.clipboard.writeText(password);
        setStatus('Copied', 'success');
    }
}

function escapeHtml(value) {
    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function escapeAttribute(value) {
    return escapeHtml(value);
}
