#!/usr/bin/env python3
"""Executable black-box API acceptance smoke, requires a running seeded server.
Usage: source .env.local; python3 scripts/smoke.py [http://127.0.0.1:5050]
No credential values are logged. Requires ONLY Python standard library.
"""
import http.cookiejar
import json
import os
import sys
from urllib.error import HTTPError
from urllib.request import Request, build_opener, HTTPCookieProcessor

base = (sys.argv[1] if len(sys.argv) > 1 else 'http://127.0.0.1:5050').rstrip('/')

class Browser:
    def __init__(self):
        self.session = build_opener(HTTPCookieProcessor(http.cookiejar.CookieJar()))
    def get(self, path):
        return self.send('GET', path)
    def send(self, method, path, body=None):
        headers = {'Accept': 'application/json'}
        if method != 'GET':
            token = self.get('/api/auth/csrf')[1]['requestToken']
            headers['X-CSRF-TOKEN'] = token
            headers['Content-Type'] = 'application/json'
        data = json.dumps(body if body is not None else {}).encode() if method != 'GET' else None
        request = Request(base + path, data=data, method=method, headers=headers)
        try:
            response = self.session.open(request, timeout=12)
            content = response.read()
            return response.status, json.loads(content) if content else {}
        except HTTPError as e:
            content = e.read()
            try: detail = json.loads(content)
            except ValueError: detail = {'title': 'Non-JSON failure'}
            return e.code, detail

def expect(status, accepted, label):
    assert status in accepted, f'{label}: expected {accepted}, observed {status}'
    print(f'OK: {label} -> HTTP {status}')

pwd1 = os.environ.get('Seed__Student1Password')
pwd2 = os.environ.get('Seed__Student2Password')
pwds = os.environ.get('Seed__StaffPassword')
if not all([pwd1,pwd2,pwds]):
    sys.exit('Missing seeded demo password environment variables; source .env.local first.')

student1, student2, staff = Browser(), Browser(), Browser()
items = student1.get('/api/equipment')
expect(items[0], {200}, 'public catalog')
free = next((x for x in items[1] if x['available']), None)
if not free:
    sys.exit('No free equipment for smoke; reset only the private demo DB or choose a fresh seeded item.')
item_id = free['id']
for browser,email,password in ((student1,'student1@celd.local',pwd1),(student2,'student2@celd.local',pwd2),(staff,'staff@celd.local',pwds)):
    status,_ = browser.send('POST','/api/auth/login',{'email':email,'password':password})
    expect(status,{200},f'login {email}')
status, reservation = student1.send('POST','/api/reservations',{'equipmentId':item_id})
expect(status,{201},'student reservation')
id = reservation['id']
expect(student2.send('POST','/api/reservations',{'equipmentId':item_id})[0],{409},'second student blocked')
expect(student2.get(f'/api/reservations/{id}')[0],{404},'other student detail concealed')
expect(student1.send('POST',f'/api/staff/reservations/{id}/pickup')[0],{403},'student cannot record pickup')
expect(staff.send('POST',f'/api/staff/reservations/{id}/pickup')[0],{200},'staff pickup')
expect(student1.send('POST',f'/api/reservations/{id}/cancel')[0],{409},'stale cancellation refused')
expect(staff.send('POST',f'/api/staff/reservations/{id}/return')[0],{200},'staff return')
expect(staff.send('POST',f'/api/staff/reservations/{id}/return')[0],{409},'repeated return refused')
status, previous = student1.get(f'/api/reservations/{id}')
expect(status,{200},'historical record preserved')
assert previous['status'] == 'Returned' and len(previous['history']) == 3
expect(student2.send('POST','/api/reservations',{'equipmentId':item_id})[0],{201},'new reservation after return')
expect(student1.send('POST','/api/auth/logout')[0],{200},'student sign-out')
expect(student1.get('/api/reservations/mine')[0],{401},'ended session blocked')
print('Smoke completed: API response checks succeeded. Record this command, UTC time, environment and Git SHA as evidence.')
