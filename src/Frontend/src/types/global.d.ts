import type { CSSProperties } from 'vue'
import { RawAxiosRequestHeaders } from 'axios'
declare global {

  declare type LocaleType = 'zh-CN' | 'en' | 'zh-HK' |  'zh-TW'

  declare type ElementPlusInfoType = 'success' | 'info' | 'warning' | 'danger'

  declare type AxiosContentType =
    | 'application/json'
    | 'application/x-www-form-urlencoded'
    | 'multipart/form-data'
    | 'text/plain'

  declare type AxiosMethod = 'get' | 'post' | 'delete' | 'put'

  declare type AxiosResponseType = 'arraybuffer' | 'blob' | 'document' | 'json' | 'text' | 'stream'

  declare interface AxiosConfig {
    params?: any
    data?: any
    url?: string
    method?: AxiosMethod
    headers?: RawAxiosRequestHeaders
    responseType?: AxiosResponseType
  }

  declare interface IResponse<T = any> {
    code: number
    data: T extends any ? T : T & any
  }
}
