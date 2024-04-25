/// <reference types="vite/client" />

class ViteEnv {
    static get VITE_DEBUG() {
        return import.meta.env.VITE_DEBUG;  
    }
    static get VITE_APP_BASE_API() {  
        return import.meta.env.VITE_APP_BASE_API;  
    }  
}

export default ViteEnv;
