const token = localStorage.getItem('token');
const urlParams = new URLSearchParams(window.location.search);
const ownerIdFromUrl = urlParams.get('ownerId');
let allRobotsData = [];

function getUserIdFromToken(token) {
    if (!token) return null;
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const payload = JSON.parse(window.atob(base64));

        return payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
            payload.nameid ||
            payload.sub ||
            payload.id;
    } catch (e) {
        console.error("Помилка парсингу токена:", e);
        return null;
    }
}

async function loadRobots() {
    try {
        const currentUserId = ownerIdFromUrl || getUserIdFromToken(token);

        if (!currentUserId) {
            document.getElementById('robotsContainer').innerHTML =
                '<p>Помилка: не вдалося визначити користувача. Перезайдіть у систему.</p>';
            return;
        }

        const response = await fetch(`https://localhost:7193/Robots/user/${currentUserId}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            allRobotsData = await response.json();
            displayRobots(allRobotsData);
        } else {
            allRobotsData = [];
            displayRobots([]);
        }
    } catch (error) {
        console.error("Серверна помилка:", error);
        document.getElementById('robotsContainer').innerHTML = '<p>Сервер не відповідає</p>';
    }
}

function filterRobots() {
    const searchQuery = document.getElementById('searchRobotName').value.toLowerCase();
    const filtered = allRobotsData.filter(r =>
        r.roboName.toLowerCase().includes(searchQuery)
    );
    displayRobots(filtered);
}

function displayRobots(robots) {
    const container = document.getElementById('robotsContainer');
    if (!container) return;

    container.innerHTML = robots.length ? '' : '<p>У цього користувача поки немає роботів.</p>';

    robots.forEach(robot => {
        const card = document.createElement('div');
        card.className = 'robot-card';
        card.innerHTML = `
            <h3>🤖 ${robot.roboName}</h3>
            <p>Статус: <b>${robot.status || 'new'}</b></p>
            <p style="font-size: 12px; color: #888;">IP: ${robot.roboIpAdress || '---'}</p>
            <button class="btn-go" onclick="startControl(${robot.roboId})">Керувати</button>
        `;
        container.appendChild(card);
    });
}

async function addNewRobot() {
    const currentUserId = ownerIdFromUrl || getUserIdFromToken(token);

    const newData = {
        roboName: document.getElementById('addName').value,
        roboIpAdress: document.getElementById('addIp').value,
        status: "new",
        streamUrl: document.getElementById('addStream').value,
        userId: parseInt(currentUserId)
    };

    if (!newData.roboName || !newData.roboIpAdress) {
        alert("Заповніть основні поля!");
        return;
    }

    const response = await fetch('https://localhost:7193/Robots', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newData)
    });

    if (response.ok) {
        alert("Робота успішно додано!");
        closeAddModal();
        loadRobots();
    } else {
        alert("Помилка при збереженні.");
    }
}

function startControl(id) {
    let url = `robot-control.html?id=${id}`;
    if (ownerIdFromUrl) url += `&ownerId=${ownerIdFromUrl}`;
    window.location.href = url;
}

function openAddModal() { document.getElementById('addModal').style.display = 'block'; }
function closeAddModal() { document.getElementById('addModal').style.display = 'none'; }