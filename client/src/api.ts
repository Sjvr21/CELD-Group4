export type Role = 'Student' | 'Staff';
export type Status = 'Reserved' | 'CheckedOut' | 'Cancelled' | 'Returned';
export interface User { id:number; displayName:string; role:Role }
export interface Equipment { id:number; assetCode:string; name:string; category:string; description:string; available:boolean }
export interface HistoryEvent { id:number; eventType:string; resultingStatus:Status; actorUserId:number; actorName:string; occurredUtc:string }
export interface Reservation {
  id:number; equipmentId:number; assetCode:string; equipmentName:string; studentId:number;
  studentName:string; status:Status; createdUtc:string; history:HistoryEvent[];
}
export class ApiFailure extends Error {
  constructor(public status:number, message:string) { super(message); this.name = 'ApiFailure'; }
}
async function request<T>(path:string, init?:RequestInit):Promise<T> {
  const response=await fetch(`/api${path}`, { credentials:'same-origin', cache:'no-store', ...init });
  if(!response.ok) {
    let message=`Request failed (${response.status}).`;
    try { const p=await response.json() as {detail?:string;title?:string}; message=p.detail||p.title||message; } catch { /* keep generic */ }
    throw new ApiFailure(response.status,message);
  }
  return await response.json() as T;
}
async function token():Promise<string> {
  const result=await request<{requestToken:string}>('/auth/csrf');
  return result.requestToken;
}
export const api={
  get:<T,>(path:string)=>request<T>(path),
  post:async <T,>(path:string,data?:unknown):Promise<T>=>{
    const csrf=await token();
    return request<T>(path, {method:'POST',headers:{'Content-Type':'application/json','X-CSRF-TOKEN':csrf},body:JSON.stringify(data??{})});
  }
};
