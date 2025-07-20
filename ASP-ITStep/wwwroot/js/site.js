class Base64 {
    static #textEncoder = new TextEncoder();
    static #textDecoder = new TextDecoder();

    // https://datatracker.ietf.org/doc/html/rfc4648#section-4
    encode = (str) => btoa(String.fromCharCode(...Base64.#textEncoder.encode(str)));
    decode = (str) => Base64.#textDecoder.decode(Uint8Array.from(atob(str), c => c.charCodeAt(0)));

    // https://datatracker.ietf.org/doc/html/rfc4648#section-5
    encodeUrl = (str) => this.encode(str).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
    decodeUrl = (str) => this.decode(str.replace(/\-/g, '+').replace(/\_/g, '/'));

    jwtEncodeBody = (header, payload) => this.encodeUrl(JSON.stringify(header)) + '.' + this.encodeUrl(JSON.stringify(payload));
    jwtDecodePayload = (jwt) => JSON.parse(this.decodeUrl(jwt.split('.')[1]));
}


document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id == 'sign-in-form') {
        e.preventDefault();
        const loginInput = form.querySelector('[name="user-login"]');
        if (!loginInput) {
            throw "'[name=user-login]' not found";
        }
        const passwordInput = form.querySelector('[name="user-password"]');
        if (!passwordInput) {
            throw "'[name=user-password]' not found";
        }
        const authAlert = document.getElementById('auth-alert');
        const authErrorMessage = document.getElementById('auth-error-message');
        const signInSpinner = document.getElementById('sign-in-spinner');
        const signInBtnText = document.getElementById('sign-in-btn-text');

        loginInput.classList.remove('is-invalid');
        passwordInput.classList.remove('is-invalid');
        authAlert.style.display = 'none';
        authErrorMessage.textContent = '';

        if (loginInput.value.length === 0) {
            loginInput.classList.add('is-invalid');
            loginInput.nextElementSibling.textContent = 'Логін не може бути порожнім';
            return;
        }
        if (passwordInput.value.length === 0) {
            passwordInput.classList.add('is-invalid');
            passwordInput.nextElementSibling.textContent = 'Пароль не може бути порожнім';
            return;
        }
        signInSpinner.style.display = 'inline-block';
        signInBtnText.textContent = 'Вхід...';

        const credentials = new Base64().encode(`${loginInput.value}:${passwordInput.value}`);

        fetch('/User/SignIn', {
            method: 'GET',
            headers: {
                'Authorization': `Basic ${credentials}`
            }
        })
            .then(r => r.json())
            .then(j => {
                signInSpinner.style.display = 'none';
                signInBtnText.textContent = 'Вхід';

                if (j.status === 200) {
                    window.location.reload();
                } else {
                    authErrorMessage.textContent = j.data || 'Помилка авторизації';
                    authAlert.style.display = 'block';
                }
            })
            .catch(err => {
                signInSpinner.style.display = 'none';
                signInBtnText.textContent = 'Вхід';
                authErrorMessage.textContent = 'Помилка мережі. Спробуйте ще раз.';
                authAlert.style.display = 'block';
            });
    }
});

document.addEventListener('DOMContentLoaded', () => {
    for (let btn of document.querySelectorAll('[data-nav]')) {
        btn.onclick = navigate;
    }
});
function navigate(e) {
    const targetBtn = e.target.closest('[data-nav]');
    const route = targetBtn.getAttribute('data-nav');
    if (!route) throw "Attribute [data-nav] not found";
    for (let btn of document.querySelectorAll('[data-nav]')) {
        btn.classList.remove("active");
    }
    targetBtn.classList.add("active");
    showPage(route);
}

function showPage(page) {
    window.activePage = page;
    const spaContainer = document.getElementById("spa-container");
    if (!spaContainer) throw "#spa-container not found";
    switch (page) {
        case 'home': spaContainer.innerHTML = `<b>Home</b>`; break;
        case 'privacy': spaContainer.innerHTML = `<b>privacy</b>`; break;
        case 'auth': spaContainer.innerHTML = !!window.accessToken ? profileHtml : authHtml; break;
        default: spaContainer.innerHTML = `<b>404</b>`;
    }
}

const profileHtml = `<div>
<h3>Вітаємо у кабінеті ! </h3>
 <button type="button" class="btn btn-success" onclick="emailClick()">Лист</button>
 <button type="button" class="btn btn-danger" onclick="exitClick()"> Вuхід </button>
</div>`;

const authHtml = `<div>
                       <div class="input-group mb-3">
                            <span class="input-group-text" id="user-login-addon"><i class="bi bi-key"></i></span>
                            <input name="user-login" type="text" class="form-control"
                                   placeholder="Логін" aria-label="Логін" aria-describedby="user-login-addon">
                            <div class="invalid-feedback"></div>
                        </div>
                        <div class="input-group mb-3">
                            <span class="input-group-text" id="user-password-addon"><i class="bi bi-lock"></i></span>
                            <input name="user-password" type="password" class="form-control" placeholder="Пароль"
                                   aria-label="Пароль" aria-describedby="user-password-addon">
                            <div class="invalid-feedback"></div>
                        </div>

                        <div id="auth-alert" class="alert alert-danger auth-alert" role="alert">
                            <i class="bi bi-exclamation-triangle me-2"></i>
                            <span id="auth-error-message"></span>
                        </div>
                       <button type="submit" class="btn btn-dark" onclick="authClick()"> Вхід </button>
</div>`;
//function startTokenWatcher() {
//    let timerRef = null;

//    function cycle() {
//        const expiry = Number(window.accessToken?.exp);
//        const now = Date.now();
//        const diff = ((expiry - now) / 1000).toFixed(1);

//        console.log(`[Token] ⏳ До завершення: ${diff}s`);

//        if (!expiry || now >= expiry) {
//            clearInterval(timerRef);
//            alert("🔒 Час на сесію завершений. Авторизуйтеся знову.");
//            window.accessToken = null;
//            exitClick();
//        }
//    }

//    if (timerRef) clearInterval(timerRef);
//    timerRef = setInterval(cycle, 1000);
//}


function emailClick() {
    fetch("/User/Email", {
        method: "POST",
        headers: {
            "Authorization": "Bearer " + window.accessToken.jti
        }
    }).then(r => r.json())
        .then(console.log);
}

function exitClick() {
    window.accessToken = null;
    showPage(window.activePage);
}

function authClick() {
    const login = document.querySelector('input[name="user-login"]').value;
    const password = document.querySelector('input[name="user-password"]').value;
    console.log(login, password);

    const credentials = new Base64().encode(`${login}:${password}`);
    fetch('/User/LogIn', {
        method: 'GET',
        headers: {
            'Authorization': `Basic ${credentials}`
        }

    }).then(r => r.json())
        .then(j => {
            if (j.status == 200) {
                window.accessToken = j.data;
                //console.log(window.accessToken);
                window.accessToken.exp = Number(j.data.exp);
                //startTokenWatcher();
                showPage(window.activePage);
            }
            else {
                alert("Rejected");
            }
        });

}