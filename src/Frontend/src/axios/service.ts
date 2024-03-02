import axios from 'axios';  
import ViteEnv from '@/helpers/ViteEnv'; 

const service = axios.create({  
  baseURL: ViteEnv.VITE_APP_BASE_API,  
  timeout: 500000 ,
  headers: {
    'Content-Type': 'application/json'
  }
});  

service.interceptors.request.use(  
  config => {  
    if (localStorage.getItem('X-MiniAuth-Token')) {  
      config.headers['X-MiniAuth-Token'] = localStorage.getItem('X-MiniAuth-Token');  
    }  
    return config;  
  },  
  error => {  
    alert(error.message || 'Error')
    console.error('Error:', error); 
    return Promise.reject(error);  
  }  
);  
  
service.interceptors.response.use(  
  response => {  
    const res = response.data;  
    if (response.status !== 200 || res.code !== 200) {  
      alert(res.message || 'Error');  
      return Promise.reject(new Error(res.message || 'Error'));  
    } else {  
      return response.data.data || response.data;  
    }  
  },  
  error => {  
    alert(error.message || 'Error');
    console.error('Error:', error);   
    return Promise.reject(error);  
  }  
);  
  
export default service;