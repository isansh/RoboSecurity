const token = localStorage.getItem('token');
const apiBase = 'https://localhost:7193/Robots';
const urlParams = new URLSearchParams(window.location.search);
const currentRobotId = urlParams.get('id');
const ownerId = urlParams.get('ownerId');

let currentRobot = null;

function initControl() {
    if (currentRobotId) {
        loadRobotData(currentRobotId);
    } else {
        alert("ID робота не вказано!");
    }
}

async function loadRobotData(id) {
    try {
        const response = await fetch(`${apiBase}/id/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!response.ok) {
            alert("Робота не знайдено");
            return;
        }

        currentRobot = await response.json();

        document.getElementById('dispName').textContent = currentRobot.roboName;
        document.getElementById('dispId').textContent = currentRobot.roboId;
        document.getElementById('dispIp').textContent = currentRobot.roboIpAdress;
        document.getElementById('dispStatus').textContent = currentRobot.status;

        if (currentRobot.streamUrl && currentRobot.streamUrl.startsWith("http")) {
            document.getElementById('videoStream').src = currentRobot.streamUrl;
        }

        document.getElementById('editName').value = currentRobot.roboName;
        document.getElementById('editIp').value = currentRobot.roboIpAdress;
        document.getElementById('editStatus').value = currentRobot.status;
        document.getElementById('editStream').value = currentRobot.streamUrl;
    } catch (error) {
        console.error("Помилка завантаження:", error);
    }
}

async function saveEdit() {
    const updateData = {
        roboId: parseInt(currentRobotId),
        roboName: document.getElementById('editName').value,
        roboIpAdress: document.getElementById('editIp').value,
        status: document.getElementById('editStatus').value,
        streamUrl: document.getElementById('editStream').value,
        userId: currentRobot.userId
    };

    const response = await fetch(`${apiBase}/edit`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(updateData)
    });

    if (response.ok) {
        alert("Дані оновлено!");
        location.reload();
    } else {
        alert("Помилка оновлення!");
    }
}

async function deleteThisRobot() {
    if (!confirm("Ви впевнені, що хочете видалити цього робота?")) return;

    const response = await fetch(`${apiBase}/${currentRobotId}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
    });

    if (response.ok) {
        alert("Робота видалено!");
        let backUrl = 'robot-selection.html';
        if (ownerId) backUrl += `?ownerId=${ownerId}`;
        window.location.href = backUrl;
    } else {
        alert("Помилка при видаленні.");
    }
}

function move(dir) {
    console.log("Робот іде:", dir);
}

function stop() {
    console.log("Стоп");
}

function openEditModal() { document.getElementById('editModal').style.display = 'block'; }
function closeModal() { document.getElementById('editModal').style.display = 'none'; }