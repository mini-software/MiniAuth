import axios from 'axios';  
import ViteEnv from '@/helpers/ViteEnv'; 
import { emitter } from '@/helpers/emitter'

const service = axios.create({  
  baseURL: ViteEnv.VITE_APP_BASE_API,  
  timeout: 50000 ,
  headers: {
    'Content-Type': 'application/json',
    'X-Requested-With': 'XMLHttpRequest',
  }
  ,withCredentials: true
});  

service.interceptors.request.use(  
  config => {  
    if (localStorage.getItem('X-MiniAuth-Token')) {  
      // authorization header token = localStorage.getItem('X-MiniAuth-Token')  
      config.headers['Authorization'] = 'Bearer ' + localStorage.getItem('X-MiniAuth-Token');
    }  
    showLoading();
    return config;  
  },  
  error => {  
    closeLoading();
    alert(error.message || 'Error')
    console.error('Error:', error); 
    return Promise.reject(error);  
  }  
);  
  
service.interceptors.response.use(  
  response => {  
    closeLoading();
    const res = response.data;  
    if (response.status !== 200 || res.code !== 200) {  
      alert(res.message || 'Error');  
      return Promise.reject(new Error(res.message || 'Error'));  
    } else {  
      return response.data.data || response.data;  
    }  
  },  
  error => {  
    closeLoading();
    const res = error.response; 
    if (error.response.status === 401  ) {
      alert('Unauthorized');
      localStorage.removeItem('X-MiniAuth-Token');
      const returnUrl = res.headers['RedirectUri'] || res.headers['redirectUri'] ||  res.headers['Location'] || res.headers['location'];
      if (returnUrl) {
        // remove returnUrl ReturnUrl url parameter
        const url = new URL(returnUrl);
        url.searchParams.delete('ReturnUrl');
        window
          .location
          .replace(url.toString());
        
        window.location.href = returnUrl + '?ReturnUrl=%2Fminiauth%2Findex.html'; // ![image](https://github.com/mini-software/MiniExcel/assets/12729184/96e69955-3e1c-4d9e-b817-d207db8932a7)

        return;
      }
      window.location.href = 'login.html' + '?ReturnUrl=%2Fminiauth%2Findex.html'; //![image](https://github.com/mini-software/MiniExcel/assets/12729184/96e69955-3e1c-4d9e-b817-d207db8932a7)

      return;
    }
    if (error.response.status === 403) {
      alert('Forbidden');
      return;
    }
      
    alert(res.data.message || error.message || 'Error');
    console.error('Error:', error);   
    return Promise.reject(error);  
  }  
);  

let acitveAxios = 0;
const showLoading = () => {
  acitveAxios++
  if (acitveAxios > 0) {
    emitter.emit('showLoading')
  }
}

const closeLoading = () => {
  acitveAxios--
  if (acitveAxios <= 0) {
    emitter.emit('closeLoading')
  }
}
  
export default service;