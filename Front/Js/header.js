function getPayload(token) {
    if (!token) return null;
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        return JSON.parse(window.atob(base64));
    } catch (e) {
        return null;
    }
}

function renderHeader() {
    const nav = document.getElementById('navLinks');
    if (!nav) return;

    const token = localStorage.getItem('token');

    if (!token) {
        nav.innerHTML = `
            <button class="btn-light-blue" onclick="location.href='login.html'">Увійти</button>
            <button class="btn-light-pink" onclick="location.href='register.html'">Реєстрація</button>
        `;
        return;
    }

    const payload = getPayload(token);
    if (!payload) {
        logout();
        return;
    }

    const role = payload.role || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    let html = '';

    if (role === "Admin" || role === "admin") {
        html = `<button class="btn-light-blue" onclick="location.href='admin.html'">Усі користувачі</button>`;
    } else {
        html = `<button class="btn-light-blue" onclick="location.href='robot-selection.html'">Мої роботи</button>`;
    }

    html += `<button class="btn-logout" onclick="logout()">Вихід</button>`;

    nav.innerHTML = html;
}

function logout() {
    localStorage.removeItem('token');
    location.href = 'index.html';
}