import './App.css';

function Home() {
    const token = localStorage.getItem('token');

    return (
        <div className="main-content">
            <h1>Ласкаво просимо</h1>
            <p>Система охорони майбутнього</p>

            <div id="welcomeActions">
                {!token ? (
                    <p className="welcome-guest-text">
                        Увійдіть в систему, щоб керувати роботами безпеки.
                    </p>
                ) : (
                    <p className="welcome-user-text">
                        Ви успішно авторизовані у системі!
                    </p>
                )}
            </div>
        </div>
    );
}

export default Home;