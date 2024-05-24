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
      config.headers['X-MiniAuth-Token'] = localStorage.getItem('X-MiniAuth-Token');  
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
    if (error.response.status === 401 ) {
      alert('Unauthorized');
      // localStorage.removeItem('X-MiniAuth-Token');
      // window.location.href = 'login.html';
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