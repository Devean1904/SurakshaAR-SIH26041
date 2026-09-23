const API = window.location.origin + '/api';
let token = localStorage.getItem('admin_token');
let adminUserId = localStorage.getItem('admin_userid');
let adminName = localStorage.getItem('admin_name');

document.addEventListener('DOMContentLoaded', () => {
    setupEventListeners();
    if (token) showDashboard();
});

function setupEventListeners() {
    document.getElementById('login-form').addEventListener('submit', e => {
        e.preventDefault();
        login();
    });
    document.querySelectorAll('.nav-btn').forEach(btn => {
        btn.addEventListener('click', () => switchTab(btn.dataset.tab));
    });
    document.getElementById('btn-logout')?.addEventListener('click', logout);
    document.getElementById('btn-refresh-workers')?.addEventListener('click', loadWorkers);
    document.getElementById('btn-refresh-escalations')?.addEventListener('click', loadEscalations);
    document.getElementById('btn-refresh-compliance')?.addEventListener('click', loadCompliance);
    document.getElementById('btn-refresh-certificates')?.addEventListener('click', loadCertificates);
    document.getElementById('btn-refresh-attempts')?.addEventListener('click', loadAttempts);
    document.getElementById('cert-filter')?.addEventListener('input', renderCertificates);
    document.getElementById('attempt-filter')?.addEventListener('input', renderAttempts);
    document.getElementById('attempt-type-filter')?.addEventListener('change', renderAttempts);
    document.getElementById('btn-show-add-manager')?.addEventListener('click', showAddManager);
    document.getElementById('btn-create-manager')?.addEventListener('click', addManager);
    document.getElementById('btn-cancel-manager')?.addEventListener('click', hideAddManager);
    document.addEventListener('click', e => {
        const btn = e.target.closest('[data-action]');
        if (!btn) return;
        const id = btn.getAttribute('data-id') || '';
        if (btn.dataset.action === 'confirm-worker') confirmWorker(id);
        if (btn.dataset.action === 'remove-worker') removeWorker(id);
        if (btn.dataset.action === 'remove-manager') removeManager(id);
        if (btn.dataset.action === 'resolve-escalation') resolveEscalation(id);
    });
}

async function login() {
    const userId = document.getElementById('login-userid').value.trim();
    const password = document.getElementById('login-password').value;
    const errorEl = document.getElementById('login-error');
    const btn = document.getElementById('login-btn');

    if (!userId || !password) { errorEl.textContent = 'Fill all fields'; return; }

    btn.textContent = 'Logging in...';
    btn.disabled = true;
    errorEl.textContent = '';

    try {
        const res = await fetch(`${API}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId, password })
        });
        const data = await res.json();

        if (!res.ok || !data.success) {
            errorEl.textContent = data.message || 'Login failed';
            return;
        }

        if (data.role !== 'Admin') {
            errorEl.textContent = 'This account is not an admin';
            return;
        }

        token = data.token;
        adminUserId = data.userId;
        adminName = data.name;

        localStorage.setItem('admin_token', token);
        localStorage.setItem('admin_userid', adminUserId);
        localStorage.setItem('admin_name', adminName);

        showDashboard();
    } catch (e) {
        errorEl.textContent = 'Network error';
    } finally {
        btn.textContent = 'Login';
        btn.disabled = false;
    }
}

function showDashboard() {
    document.getElementById('login-screen').classList.remove('active');
    document.getElementById('dashboard-screen').classList.add('active');
    document.getElementById('admin-name').textContent = adminName || adminUserId;
    loadOverview();
}

function logout() {
    token = null;
    adminUserId = null;
    adminName = null;
    localStorage.removeItem('admin_token');
    localStorage.removeItem('admin_userid');
    localStorage.removeItem('admin_name');
    document.getElementById('dashboard-screen').classList.remove('active');
    document.getElementById('login-screen').classList.add('active');
    document.getElementById('login-form').reset();
}

function switchTab(tab) {
    document.querySelectorAll('.nav-btn').forEach(b => b.classList.remove('active'));
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
    document.querySelector(`[data-tab="${tab}"]`).classList.add('active');
    document.getElementById(`tab-${tab}`).classList.add('active');

    if (tab === 'overview') loadOverview();
    if (tab === 'workers') loadWorkers();
    if (tab === 'managers') loadManagers();
    if (tab === 'escalations') loadEscalations();
    if (tab === 'sites') loadSites();
    if (tab === 'compliance') loadCompliance();
    if (tab === 'certificates') loadCertificates();
    if (tab === 'attempts') loadAttempts();
}

function headers() {
    return { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` };
}

async function loadOverview() {
    try {
        const [workersRes, mgrsRes, pendingRes, escRes] = await Promise.all([
            fetch(`${API}/admin/workers`, { headers: headers() }),
            fetch(`${API}/admin/managers`, { headers: headers() }),
            fetch(`${API}/admin/workers/pending`, { headers: headers() }),
            fetch(`${API}/admin/escalations`, { headers: headers() })
        ]);

        const workers = await workersRes.json();
        const managers = await mgrsRes.json();
        const pending = await pendingRes.json();
        const escalations = await escRes.json();

        document.getElementById('stat-workers').textContent = workers.length || 0;
        document.getElementById('stat-managers').textContent = managers.length || 0;
        document.getElementById('stat-pending').textContent = pending.length || 0;
        document.getElementById('stat-escalations').textContent =
            (Array.isArray(escalations) ? escalations.filter(e => e.status === 'Active').length : 0);
    } catch (e) {
        console.error('loadOverview error:', e);
    }
}

async function loadWorkers() {
    try {
        const [allRes, pendingRes] = await Promise.all([
            fetch(`${API}/admin/workers`, { headers: headers() }),
            fetch(`${API}/admin/workers/pending`, { headers: headers() })
        ]);

        const pending = await pendingRes.json();
        const all = await allRes.json();

        const pendingTbody = document.querySelector('#pending-table tbody');
        pendingTbody.innerHTML = '';
        document.getElementById('pending-empty').style.display = pending.length ? 'none' : 'block';

        pending.forEach(w => {
            pendingTbody.innerHTML += `<tr>
                <td>${esc(w.userId)}</td>
                <td>${esc(w.name)}</td>
                <td>${esc(w.phoneNumber || '-')}</td>
                <td>${esc(w.enrolledByManagerId || '-')}</td>
                <td><button class="btn btn-success btn-sm" data-action="confirm-worker" data-id="${escAttr(w.userId)}">Confirm</button></td>
            </tr>`;
        });

        const allTbody = document.querySelector('#workers-table tbody');
        allTbody.innerHTML = '';
        document.getElementById('workers-empty').style.display = all.length ? 'none' : 'block';

        all.forEach(w => {
            allTbody.innerHTML += `<tr>
                <td>${esc(w.userId)}</td>
                <td>${esc(w.name)}</td>
                <td>${esc(w.phoneNumber || '-')}</td>
                <td>${w.awaitingAdminConfirmation ? '<span style="color:var(--warning)">Pending</span>' : '<span style="color:var(--success)">Active</span>'}</td>
                <td>${esc(w.enrolledByManagerId || '-')}</td>
                <td><button class="btn btn-danger btn-sm" data-action="remove-worker" data-id="${escAttr(w.userId)}">Remove</button></td>
            </tr>`;
        });
    } catch (e) {
        console.error('loadWorkers error:', e);
    }
}

async function confirmWorker(workerId) {
    try {
        await fetch(`${API}/admin/worker/confirm`, {
            method: 'POST',
            headers: headers(),
            body: JSON.stringify({ workerId })
        });
        loadWorkers();
    } catch (e) { alert('Failed'); }
}

async function removeWorker(userId) {
    if (!confirm(`Remove worker ${userId}?`)) return;
    try {
        await fetch(`${API}/admin/worker/remove`, {
            method: 'POST',
            headers: headers(),
            body: JSON.stringify({ userId })
        });
        loadWorkers();
    } catch (e) { alert('Failed'); }
}

function showAddManager() { document.getElementById('add-manager-form').style.display = 'block'; }
function hideAddManager() { document.getElementById('add-manager-form').style.display = 'none'; }

async function addManager() {
    const userId = document.getElementById('mgr-userid').value.trim();
    const name = document.getElementById('mgr-name').value.trim();
    const phone = document.getElementById('mgr-phone').value.trim();
    const password = document.getElementById('mgr-password').value;

    if (!userId || !name || !password) { alert('Fill required fields'); return; }

    try {
        const res = await fetch(`${API}/admin/manager/add`, {
            method: 'POST',
            headers: headers(),
            body: JSON.stringify({ userId, name, phoneNumber: phone, password })
        });
        const data = await res.json();
        if (!res.ok) { alert(data.message || 'Failed'); return; }
        hideAddManager();
        document.getElementById('mgr-userid').value = '';
        document.getElementById('mgr-name').value = '';
        document.getElementById('mgr-phone').value = '';
        document.getElementById('mgr-password').value = '';
        loadManagers();
    } catch (e) { alert('Network error'); }
}

async function loadManagers() {
    try {
        const res = await fetch(`${API}/admin/managers`, { headers: headers() });
        const managers = await res.json();
        const tbody = document.querySelector('#managers-table tbody');
        tbody.innerHTML = '';
        document.getElementById('managers-empty').style.display = managers.length ? 'none' : 'block';

        managers.forEach(m => {
            tbody.innerHTML += `<tr>
                <td>${esc(m.userId)}</td>
                <td>${esc(m.name)}</td>
                <td>${esc(m.phoneNumber || '-')}</td>
                <td>${(m.assignedWorkerIds || []).length}</td>
                <td>${m.createdAt ? new Date(m.createdAt).toLocaleDateString() : '-'}</td>
                <td><button class="btn btn-danger btn-sm" data-action="remove-manager" data-id="${escAttr(m.userId)}">Remove</button></td>
            </tr>`;
        });
    } catch (e) { console.error('loadManagers error:', e); }
}

async function removeManager(userId) {
    if (!confirm(`Remove manager ${userId}?`)) return;
    try {
        await fetch(`${API}/admin/manager/remove`, {
            method: 'POST',
            headers: headers(),
            body: JSON.stringify({ userId })
        });
        loadManagers();
    } catch (e) { alert('Failed'); }
}

async function loadEscalations() {
    try {
        const res = await fetch(`${API}/admin/escalations`, { headers: headers() });
        const escalations = await res.json();
        const tbody = document.querySelector('#escalations-table tbody');
        tbody.innerHTML = '';
        document.getElementById('escalations-empty').style.display =
            (Array.isArray(escalations) && escalations.length) ? 'none' : 'block';

        (Array.isArray(escalations) ? escalations : []).forEach(e => {
            const statusColor = e.status === 'Active' ? 'var(--danger)' : 'var(--success)';
            tbody.innerHTML += `<tr>
                <td style="font-size:11px">${esc(e.id || '-')}</td>
                <td>${esc(e.workerId || '-')}</td>
                <td>${esc(e.severity || '-')}</td>
                <td style="max-width:200px;overflow:hidden;text-overflow:ellipsis">${esc(e.description || '-')}</td>
                <td style="color:${statusColor}">${esc(e.status || '-')}</td>
                <td>${e.reportedAt ? new Date(e.reportedAt).toLocaleDateString() : '-'}</td>
                <td>${e.status === 'Active' ?
                    `<button class="btn btn-success btn-sm" data-action="resolve-escalation" data-id="${escAttr(e.id)}">Resolve</button>` :
                    'Resolved'}</td>
            </tr>`;
        });
    } catch (e) { console.error('loadEscalations error:', e); }
}

async function resolveEscalation(reportId) {
    try {
        await fetch(`${API}/admin/escalation/resolve`, {
            method: 'POST',
            headers: headers(),
            body: JSON.stringify({ reportId })
        });
        loadEscalations();
    } catch (e) { alert('Failed'); }
}

async function loadSites() {
    try {
        const res = await fetch(`${API}/admin/sites`, { headers: headers() });
        const sites = await res.json();
        const tbody = document.querySelector('#sites-table tbody');
        tbody.innerHTML = '';
        const emptyEl = document.getElementById('sites-empty');
        const list = Array.isArray(sites) ? sites : [];
        if (emptyEl) emptyEl.style.display = list.length ? 'none' : 'block';

        list.forEach(s => {
            tbody.innerHTML += `<tr>
                <td>${esc(s.id || '-')}</td>
                <td>${esc(s.siteName || '-')}</td>
                <td>${esc(s.adminId || '-')}</td>
                <td>${(s.scenarioPoints || []).length}</td>
                <td>${s.recordedAt ? new Date(s.recordedAt).toLocaleDateString() : '-'}</td>
            </tr>`;
        });
    } catch (e) { console.error('loadSites error:', e); }
}

async function loadCompliance() {
    try {
        const res = await fetch(`${API}/admin/compliance`, { headers: headers() });
        if (!res.ok) {
            const err = await res.json().catch(() => ({}));
            alert(err.message || 'Failed to load compliance data');
            return;
        }
        const data = await res.json();

        document.getElementById('comp-pass-rate').textContent = data.passRate ?? 0;
        document.getElementById('comp-assessments').textContent = data.totalAssessments ?? 0;
        document.getElementById('comp-certificates').textContent = data.totalCertificates ?? 0;
        document.getElementById('comp-failed').textContent = data.failedAssessments ?? 0;
        const attTotal = document.getElementById('comp-attempts-total');
        const attDone = document.getElementById('comp-attempts-done');
        if (attTotal) attTotal.textContent = data.totalAttempts ?? 0;
        if (attDone) attDone.textContent = data.completedAttempts ?? 0;

        const modTbody = document.querySelector('#comp-modules-table tbody');
        modTbody.innerHTML = '';
        const modules = Array.isArray(data.byModule) ? data.byModule : [];
        document.getElementById('comp-modules-empty').style.display = modules.length ? 'none' : 'block';
        modules.forEach(m => {
            modTbody.innerHTML += `<tr>
                <td>${esc(m.moduleId)}</td>
                <td>${m.attempts}</td>
                <td>${m.passed}</td>
                <td>${m.avgScore}</td>
            </tr>`;
        });

        const certTbody = document.querySelector('#comp-certs-table tbody');
        certTbody.innerHTML = '';
        const certs = Array.isArray(data.recentCertificates) ? data.recentCertificates : [];
        document.getElementById('comp-certs-empty').style.display = certs.length ? 'none' : 'block';
        certs.forEach(c => {
            certTbody.innerHTML += `<tr>
                <td style="font-size:11px">${esc(c.certificateId)}</td>
                <td>${esc(c.employeeName || c.employeeId)}</td>
                <td>${esc(c.moduleName || c.moduleId)}</td>
                <td>${c.score}</td>
                <td>${c.issuedAt ? new Date(c.issuedAt).toLocaleDateString() : '-'}</td>
            </tr>`;
        });

        const attTbody = document.querySelector('#comp-attempts-table tbody');
        attTbody.innerHTML = '';
        const attempts = Array.isArray(data.recentAssessments) ? data.recentAssessments : [];
        document.getElementById('comp-attempts-empty').style.display = attempts.length ? 'none' : 'block';
        attempts.forEach(a => {
            const resultColor = a.passed ? 'var(--success)' : 'var(--danger)';
            attTbody.innerHTML += `<tr>
                <td>${esc(a.employeeId)}</td>
                <td>${esc(a.moduleId)}</td>
                <td>${a.actionScore}</td>
                <td>${a.questionScore}</td>
                <td>${a.totalScore}</td>
                <td style="color:${resultColor}">${a.passed ? 'PASS' : 'FAIL'}</td>
                <td>${a.submittedAt ? new Date(a.submittedAt).toLocaleString() : '-'}</td>
            </tr>`;
        });
    } catch (e) {
        console.error('loadCompliance error:', e);
    }
}

let certificatesCache = [];
let attemptsCache = [];

async function loadCertificates() {
    try {
        const res = await fetch(`${API}/admin/certificates`, { headers: headers() });
        if (!res.ok) {
            const err = await res.json().catch(() => ({}));
            alert(err.message || 'Failed to load certificates');
            return;
        }
        certificatesCache = await res.json();
        if (!Array.isArray(certificatesCache)) certificatesCache = [];
        renderCertificates();
    } catch (e) {
        console.error('loadCertificates error:', e);
    }
}

function renderCertificates() {
    const q = (document.getElementById('cert-filter')?.value || '').toLowerCase();
    const list = certificatesCache.filter(c => {
        if (!q) return true;
        return [c.certificateId, c.employeeId, c.employeeName, c.moduleId, c.moduleName]
            .some(v => String(v || '').toLowerCase().includes(q));
    });

    const countEl = document.getElementById('cert-count');
    const avgEl = document.getElementById('cert-avg-score');
    if (countEl) countEl.textContent = certificatesCache.length;
    if (avgEl) {
        avgEl.textContent = certificatesCache.length
            ? Math.round(certificatesCache.reduce((s, c) => s + (c.score || 0), 0) / certificatesCache.length)
            : '—';
    }

    const tbody = document.querySelector('#certificates-table tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    const empty = document.getElementById('certificates-empty');
    if (empty) empty.style.display = list.length ? 'none' : 'block';

    list.forEach(c => {
        const tx = c.blockchainTx ? String(c.blockchainTx).slice(0, 10) + '…' : '-';
        const verifyUrl = c.qrPayload ? `/verify/?payload=${encodeURIComponent(c.qrPayload)}` : '/verify/';
        tbody.innerHTML += `<tr>
            <td style="font-size:11px">${esc(c.certificateId)}</td>
            <td>${esc(c.employeeName || c.employeeId)}<br><span style="color:var(--text-dim);font-size:11px">${esc(c.employeeId)}</span></td>
            <td>${esc(c.moduleName || c.moduleId)}</td>
            <td>${c.score}</td>
            <td style="color:${c.passed ? 'var(--success)' : 'var(--danger)'}">${c.passed ? 'PASS' : 'FAIL'}</td>
            <td>${c.issuedAt ? new Date(c.issuedAt).toLocaleString() : '-'}</td>
            <td style="font-size:11px" title="${escAttr(c.blockchainTx || '')}">${esc(tx)}</td>
            <td><a class="btn btn-sm" href="${escAttr(verifyUrl)}" target="_blank" rel="noopener">Verify</a></td>
        </tr>`;
    });
}

async function loadAttempts() {
    try {
        const res = await fetch(`${API}/admin/attempts`, { headers: headers() });
        if (!res.ok) {
            const err = await res.json().catch(() => ({}));
            alert(err.message || 'Failed to load attempts');
            return;
        }
        const data = await res.json();
        if (Array.isArray(data)) {
            attemptsCache = data.map(a => ({
                type: 'assessment',
                id: a.assessmentId || '',
                ...a
            }));
        } else {
            const items = Array.isArray(data.items) ? data.items
                : [...(data.assessments || []), ...(data.trainingAttempts || [])];
            attemptsCache = items;
        }
        renderAttempts();
    } catch (e) {
        console.error('loadAttempts error:', e);
    }
}

function renderAttempts() {
    const q = (document.getElementById('attempt-filter')?.value || '').toLowerCase();
    const type = document.getElementById('attempt-type-filter')?.value || '';
    const list = attemptsCache.filter(a => {
        if (type && a.type !== type) return false;
        if (!q) return true;
        return [a.id, a.attemptId, a.employeeId, a.moduleId, a.scenarioId, a.status]
            .some(v => String(v || '').toLowerCase().includes(q));
    });

    const trainingCount = attemptsCache.filter(a => a.type === 'training').length;
    const assessmentCount = attemptsCache.filter(a => a.type === 'assessment').length;
    const notOk = attemptsCache.filter(a => !a.passed && a.status !== 'completed' && a.status !== 'passed').length;
    const set = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };
    set('att-total', attemptsCache.length);
    set('att-training', trainingCount);
    set('att-assessment', assessmentCount);
    set('att-failed', notOk);

    const tbody = document.querySelector('#attempts-table tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    const empty = document.getElementById('attempts-empty');
    if (empty) empty.style.display = list.length ? 'none' : 'block';

    list.forEach(a => {
        const isTraining = a.type === 'training';
        const resultColor = a.passed ? 'var(--success)'
            : (a.status === 'in_progress' ? 'var(--warning)' : 'var(--danger)');
        const resultLabel = a.status === 'in_progress' ? 'IN PROGRESS'
            : (a.passed ? 'PASS' : (a.status === 'failed' || a.status === 'completed' || a.type === 'assessment'
                ? (a.passed ? 'PASS' : 'FAIL') : (a.status || '-')));
        const when = a.endTime || a.submittedAt || a.startTime || '';
        tbody.innerHTML += `<tr>
            <td><span class="badge">${isTraining ? 'TRAIN' : 'ASSESS'}</span></td>
            <td style="font-size:11px">${esc(a.id || a.attemptId || a.assessmentId || '-')}</td>
            <td>${esc(a.employeeId)}</td>
            <td>${esc(a.moduleId)}${a.scenarioId ? `<br><span style="color:var(--text-dim);font-size:11px">${esc(a.scenarioId)}</span>` : ''}</td>
            <td>${a.actionScore ?? 0}</td>
            <td>${a.questionScore ?? 0}</td>
            <td>${a.totalScore ?? 0}</td>
            <td style="color:${resultColor}">${esc(resultLabel)}</td>
            <td>${when ? new Date(when).toLocaleString() : '-'}</td>
        </tr>`;
    });
}

function esc(str) {
    if (str === null || str === undefined) return '';
    const div = document.createElement('div');
    div.textContent = String(str);
    return div.innerHTML;
}

function escAttr(str) {
    return String(str ?? '')
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}