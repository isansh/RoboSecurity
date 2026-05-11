const token = localStorage.getItem('token');
const apiBase = 'https://localhost:7193/Users';

if (!token) {
    window.location.href = 'login.html';
}

function logout() {
    localStorage.removeItem('token');
    window.location.href = 'login.html';
}

async function loadAllUsers() {
    try {
        const response = await fetch(apiBase, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
            renderUsers(await response.json());
        } else {
            console.error("Не вдалося завантажити користувачів");
        }
    } catch (error) {
        console.error("Помилка мережі:", error);
    }
}

async function searchUser() {
    const mail = document.getElementById('searchEmail').value;
    if (!mail) return alert("Введіть email");

    try {
        const response = await fetch(`${apiBase}/${mail}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
            const user = await response.json();
            renderUsers([user]);
        } else {
            alert("Користувача не знайдено");
        }
    } catch (error) {
        alert("Помилка пошуку");
    }
}

function renderUsers(users) {
    const tbody = document.getElementById('usersBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    users.forEach(u => {
        tbody.innerHTML += `
            <tr>
                <td>${u.userId}</td>
                <td>${u.userMail}</td>
                <td>${u.userRoleName}</td>
                <td>
                    <button class="view-btn" onclick="viewUserRobots(${u.userId})">Роботи</button>
                    <button class="edit-btn" onclick="openEditModal(${u.userId}, '${u.userMail}', '${u.userRoleName}')">Редагувати</button>
                    <button class="del-btn" onclick="deleteUser(${u.userId})">Видалити</button>
                </td>
            </tr>`;
    });
}

async function deleteUser(id) {
    if (confirm("Видалити користувача?")) {
        const response = await fetch(`${apiBase}/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) loadAllUsers();
    }
}

function openEditModal(id, mail, roleName) {
    document.getElementById('editUserId').value = id;
    document.getElementById('editEmail').value = mail;
    document.getElementById('editRoleId').value = (roleName === "Admin") ? 1 : 2;
    document.getElementById('editPassword').value = "";
    document.getElementById('editConfirmPassword').value = "";
    document.getElementById('editModal').style.display = 'block';
}

function closeModal() {
    document.getElementById('editModal').style.display = 'none';
}

async function saveUserEdit() {
    const data = {
        userId: parseInt(document.getElementById('editUserId').value),
        userMail: document.getElementById('editEmail').value,
        userRoleId: parseInt(document.getElementById('editRoleId').value),
        password: document.getElementById('editPassword').value,
        confirmPassword: document.getElementById('editConfirmPassword').value
    };

    if (data.password !== data.confirmPassword) return alert("Паролі не співпадають!");

    const response = await fetch(`${apiBase}/edit`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });

    if (response.ok) {
        alert("Оновлено!");
        closeModal();
        loadAllUsers();
    } else {
        alert("Помилка оновлення");
    }
}

function viewUserRobots(userId) {
    window.location.href = `robot-selection.html?ownerId=${userId}`;
}

loadAllUsers();