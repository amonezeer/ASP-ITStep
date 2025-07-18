﻿class Base64 {
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

function navigate(route) {
    const spaContainer = document.getElementById("spa-container");
    if (!spaContainer) throw "#spa-container not found";
    switch (route) {
        case 'home': spaContainer.innerHTML = `<b>Home</b>`; break;
        case 'privacy': spaContainer.innerHTML = `<b>privacy</b>`; break;
        case 'auth': spaContainer.innerHTML = `<b>auth</b>`; break;
        default: spaContainer.innerHTML = `<b>404</b>`; 
    }
}