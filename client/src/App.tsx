import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { api, type Equipment, type Reservation, type Status, type User } from './api';

type Tab = 'catalog' | 'mine' | 'staff';
const readableStatus: Record<Status,string> = {
  Reserved:'Reserved · awaiting pickup', CheckedOut:'Checked out', Cancelled:'Cancelled', Returned:'Returned'
};
function dateTime(value:string){ return new Date(value.endsWith('Z')?value:value+'Z').toLocaleString(); }
function errorText(error:unknown){ return error instanceof Error?error.message:'An unexpected error occurred.'; }

export default function App(){
  const [user,setUser]=useState<User|null>(null);
  const [checking,setChecking]=useState(true);
  const [tab,setTab]=useState<Tab>('catalog');
  const [items,setItems]=useState<Equipment[]>([]);
  const [categories,setCategories]=useState<string[]>([]);
  const [search,setSearch]=useState('');
  const [category,setCategory]=useState('');
  const [item,setItem]=useState<Equipment|null>(null);
  const [reservations,setReservations]=useState<Reservation[]>([]);
  const [chosen,setChosen]=useState<Reservation|null>(null);
  const [staffStatus,setStaffStatus]=useState('');
  const [loading,setLoading]=useState(false);
  const [busy,setBusy]=useState(false);
  const [error,setError]=useState('');
  const [notice,setNotice]=useState('');
  const [email,setEmail]=useState('');
  const [password,setPassword]=useState('');
  const [signingIn,setSigningIn]=useState(false);

  const reloadCatalog=useCallback(async (s=search,c=category)=>{
    const qs=new URLSearchParams();if(s.trim())qs.set('search',s.trim());if(c)qs.set('category',c);
    setLoading(true);
    try {const data=await api.get<Equipment[]>(`/equipment?${qs.toString()}`);setItems(data);
      if(item){const matching=data.find(x=>x.id===item.id);if(matching)setItem(matching);}
    } catch(e){setError(errorText(e));setItems([]);} finally{setLoading(false);}
  },[search,category,item]);

  const reloadReservations=useCallback(async (actor:User|null=user, filter=staffStatus)=>{
    if(!actor)return;
    setLoading(true);
    try{
      const path=actor.role==='Staff'?`/staff/reservations${filter?`?status=${encodeURIComponent(filter)}`:''}`:'/reservations/mine';
      const data=await api.get<Reservation[]>(path);
      setReservations(data);setError('');
      if(chosen){setChosen(data.find(x=>x.id===chosen.id)??null);}
    }catch(e){setError(errorText(e));setReservations([]);}finally{setLoading(false);}
  },[user,staffStatus,chosen]);

  useEffect(()=>{
    let active=true;
    Promise.allSettled([api.get<User>('/auth/me'),api.get<Equipment[]>('/equipment'),api.get<string[]>('/equipment/categories')])
      .then(results=>{if(!active)return;
        if(results[0].status==='fulfilled')setUser(results[0].value);
        if(results[1].status==='fulfilled')setItems(results[1].value);
        else setError('Unable to load equipment catalog. Try refreshing.');
        if(results[2].status==='fulfilled')setCategories(results[2].value);
      }).finally(()=>{if(active)setChecking(false)});
    return ()=>{active=false;};
  },[]);

  async function signIn(e:FormEvent){e.preventDefault();setSigningIn(true);setError('');setNotice('');
    try{const next=await api.post<User>('/auth/login',{email,password});setUser(next);setPassword('');setTab('catalog');
      setNotice(`Signed in as ${next.displayName}.`);}
    catch(err){setError(errorText(err));}
    finally{setSigningIn(false);}
  }
  async function signOut(){setBusy(true);setError('');
    try{await api.post('/auth/logout');setUser(null);setChosen(null);setReservations([]);setTab('catalog');setNotice('Signed out.');}
    catch(e){setError(errorText(e));}finally{setBusy(false);}
  }
  function openTab(next:Tab){setTab(next);setChosen(null);setError('');setNotice('');
    if(next==='catalog')void reloadCatalog();else void reloadReservations();
  }
  async function openItem(id:number){setBusy(true);setError('');
    try{setItem(await api.get<Equipment>(`/equipment/${id}`));}
    catch(e){setError(errorText(e));setItem(null);}finally{setBusy(false);}
  }
  async function openReservation(id:number){setBusy(true);setError('');
    try{setChosen(await api.get<Reservation>(`/reservations/${id}`));}
    catch(e){setError(errorText(e));setChosen(null);}finally{setBusy(false);}
  }
  async function operation(path:string,success:string){setBusy(true);setError('');setNotice('');
    try{
      await api.post<Reservation>(path);
      // Display success only after server response confirms accepted operation.
      setNotice(success);
    }catch(e){setError(`${errorText(e)} Refreshing server state.`);}
    finally{
      setBusy(false);
      const fresh=await Promise.allSettled([api.get<Equipment[]>('/equipment'),user?
        api.get<Reservation[]>(user.role==='Staff'?`/staff/reservations${staffStatus?`?status=${staffStatus}`:''}`:'/reservations/mine'):Promise.resolve([])]);
      if(fresh[0].status==='fulfilled'){setItems(fresh[0].value);if(item){setItem(fresh[0].value.find(x=>x.id===item.id)??null);}}
      if(fresh[1].status==='fulfilled')setReservations(fresh[1].value);
      if(chosen){try{setChosen(await api.get<Reservation>(`/reservations/${chosen.id}`));}catch{setChosen(null);}}
    }
  }
  function filterCatalog(e:FormEvent){e.preventDefault();setItem(null);void reloadCatalog(search,category);}
  function filterStaff(e:FormEvent){e.preventDefault();setChosen(null);void reloadReservations(user,staffStatus);}

  return <div className="app">
    <header className="topbar"><div className="topbar-content"><div className="brand"><span className="logo" aria-hidden="true">C</span>
      <div><strong>CELD</strong><small>Campus Equipment Lending Desk</small></div></div>
      <div className="account">{user?<><span className="user-chip">{user.displayName} · {user.role}</span>
        <button className="btn-outline" onClick={()=>void signOut()} disabled={busy}>Sign out</button></>:<span className="tag">Public catalog</span>}</div>
    </div></header>
    <main className="layout">
      <section className="hero"><div className="eyebrow">CAMPUS SERVICES / EQUIPMENT</div>
        <h1>Equipment when you need it.</h1>
        <p>Browse campus equipment, reserve available items, and follow every stage of your lending request.</p></section>
      <div className="main-grid">
        <div className="workspace">
          <nav className="tabs" aria-label="Main navigation">
            <button className={tab==='catalog'?'selected':''} onClick={()=>openTab('catalog')} aria-current={tab==='catalog'?'page':undefined}>Equipment catalog</button>
            {user?.role==='Student'&&<button className={tab==='mine'?'selected':''} onClick={()=>openTab('mine')} aria-current={tab==='mine'?'page':undefined}>My reservations</button>}
            {user?.role==='Staff'&&<button className={tab==='staff'?'selected':''} onClick={()=>openTab('staff')} aria-current={tab==='staff'?'page':undefined}>Staff desk</button>}
          </nav>
          <div className="messages" role="status" aria-live="polite">{notice&&<p className="success">{notice}</p>}{error&&<p className="failure" role="alert">{error}</p>}</div>
          {tab==='catalog'&&<section aria-label="Equipment catalog">
            <form className="filters" onSubmit={filterCatalog}>
              <label>Search by name<input value={search} onChange={e=>setSearch(e.target.value)} placeholder="Search equipment..."/></label>
              <label>Category<select value={category} onChange={e=>setCategory(e.target.value)}><option value="">All categories</option>
                {categories.map(x=><option key={x} value={x}>{x}</option>)}</select></label>
              <button type="submit" disabled={loading}>Apply filters</button>
            </form>
            <div className="section-heading"><h2>Equipment</h2><span>{items.length} item{items.length===1?'':'s'}</span></div>
            {loading?<p className="state">Loading equipment...</p>:items.length===0?<p className="state">No equipment matched your filters, or the catalog could not be loaded.</p>:
              <div className="equipment-list">{items.map(x=><article className="equipment-row" key={x.id}>
                <div className="tile-symbol" aria-hidden="true">{x.category==='Computers'?'⌘':x.category==='Cameras'?'◎':x.category==='Audio'?'◉':'▣'}</div>
                <div className="grow"><h3>{x.name}</h3><p>{x.category} · {x.assetCode}</p></div>
                <span className={x.available?'status available':'status unavailable'}>{x.available?'Available':'Unavailable'}</span>
                <button className="btn-outline" onClick={()=>void openItem(x.id)} disabled={busy}>View details</button>
              </article>)}</div>}
          </section>}
          {(tab==='mine'||tab==='staff')&&<section aria-label="Reservations">
            <div className="section-heading"><h2>{tab==='mine'?'My reservations':'Staff reservations'}</h2><span>{reservations.length} records</span></div>
            {tab==='staff'&&<form className="filters compact" onSubmit={filterStaff}><label>Reservation status
              <select value={staffStatus} onChange={e=>setStaffStatus(e.target.value)}><option value="">All statuses</option>
                {(['Reserved','CheckedOut','Cancelled','Returned'] as Status[]).map(x=><option key={x} value={x}>{x}</option>)}</select></label>
              <button type="submit">Apply filter</button></form>}
            {loading?<p className="state">Loading reservations...</p>:reservations.length===0?<p className="state">No reservations found for this view.</p>:
              <div className="reservation-list">{reservations.map(r=><article className="reservation-row" key={r.id}>
                <div><h3>{r.equipmentName}</h3><p>#{r.id} · {r.assetCode} · {tab==='staff'?`${r.studentName} · `:''}{dateTime(r.createdUtc)}</p></div>
                <span className="status neutral">{readableStatus[r.status]}</span>
                <button className="btn-outline" disabled={busy} onClick={()=>void openReservation(r.id)}>Inspect</button>
              </article>)}</div>}
          </section>}
        </div>
        <aside className="sidebar">
          {!user&&!checking?<section className="panel"><div className="panel-eyebrow">STUDENT / STAFF ACCESS</div><h2>Sign in</h2>
            <p>Use your provisioned campus lending account.</p>
            <form onSubmit={signIn} className="signin">{error&&<p className="failure" role="alert">{error}</p>}<label>Email<input type="email" autoComplete="username" required value={email} onChange={e=>setEmail(e.target.value)} placeholder="email@celd.local"/></label>
              <label>Password<input type="password" autoComplete="current-password" required value={password} onChange={e=>setPassword(e.target.value)}/></label>
              <button type="submit" disabled={signingIn}>{signingIn?'Signing in...':'Sign in'}</button></form>
          </section>:<section className="panel"><div className="panel-eyebrow">ACCOUNT</div><h2>{checking?'Checking session...':`Hello, ${user?.displayName}`}</h2>
            <p>{user?.role==='Staff'?'Manage staff pickup and return workflows.':user?.role==='Student'?'Reserve equipment and follow your borrowing history.':'Browse equipment freely.'}</p>
            {user&&<span className="tag">Role: {user.role}</span>}</section>}
          <section className="panel tips"><div className="panel-eyebrow">HOW IT WORKS</div><h2>One item. One active reservation.</h2>
            <p>Each equipment item can be reserved by one student at a time. Staff record pickup and return; all successful changes remain in the history.</p></section>
        </aside>
      </div>
    </main>
    {item&&<div className="overlay" role="presentation" onMouseDown={e=>{if(e.target===e.currentTarget)setItem(null)}}>
      <section className="modal" role="dialog" aria-modal="true" aria-label="Equipment details"><button className="close" aria-label="Close equipment details" onClick={()=>setItem(null)}>×</button>
        <div className="panel-eyebrow">{item.category} / {item.assetCode}</div>{error&&<p className="failure" role="alert">{error}</p>}{notice&&<p className="success">{notice}</p>}<h2>{item.name}</h2><p>{item.description}</p>
        <p className="detail-status"><strong>Current status: </strong>{item.available?'Available':'Unavailable'}</p>
        {user?.role==='Student'&&<button disabled={!item.available||busy} onClick={()=>void reserveItem(item.id)} className="primary-wide">{busy?'Processing...':'Reserve equipment'}</button>}
        {!user&&<p className="note">Sign in as a Student to reserve available equipment.</p>}
        {user?.role==='Staff'&&<p className="note">Staff accounts cannot create student reservations.</p>}
      </section></div>}
    {chosen&&<div className="overlay" role="presentation" onMouseDown={e=>{if(e.target===e.currentTarget)setChosen(null)}}>
      <section className="modal" role="dialog" aria-modal="true" aria-label="Reservation details"><button className="close" aria-label="Close reservation details" onClick={()=>setChosen(null)}>×</button>
        <div className="panel-eyebrow">RESERVATION #{chosen.id} / {chosen.assetCode}</div>{error&&<p className="failure" role="alert">{error}</p>}{notice&&<p className="success">{notice}</p>}
        <h2>{chosen.equipmentName}</h2><p>Borrower: {chosen.studentName}</p><p>Created: {dateTime(chosen.createdUtc)}</p>
        <p className="detail-status"><strong>Status: </strong>{readableStatus[chosen.status]}</p>
        <div className="action-row">
          {user?.role==='Student'&&chosen.status==='Reserved'&&<button disabled={busy} onClick={()=>void operation(`/reservations/${chosen.id}/cancel`,'Reservation cancelled.')}>Cancel reservation</button>}
          {user?.role==='Staff'&&chosen.status==='Reserved'&&<button disabled={busy} onClick={()=>void operation(`/staff/reservations/${chosen.id}/pickup`,'Pickup recorded.')}>Record pickup</button>}
          {user?.role==='Staff'&&chosen.status==='CheckedOut'&&<button disabled={busy} onClick={()=>void operation(`/staff/reservations/${chosen.id}/return`,'Return recorded.')}>Record return</button>}
        </div>
        <h3>Reservation history</h3>
        <ol className="history">{chosen.history.map(h=><li key={h.id}><strong>{h.eventType}</strong> → {h.resultingStatus}
          <span>{h.actorName} · {dateTime(h.occurredUtc)}</span></li>)}</ol>
      </section></div>}
  </div>;

  async function reserveItem(equipmentId:number){
    setBusy(true);setError('');setNotice('');
    try{const created=await api.post<Reservation>('/reservations',{equipmentId});
      setNotice(`Reservation #${created.id} created successfully.`);setItem(null);
    }catch(e){setError(`${errorText(e)} Refreshing current availability.`);}
    finally{setBusy(false);await reloadCatalog();setItem(null);}
  }
}
