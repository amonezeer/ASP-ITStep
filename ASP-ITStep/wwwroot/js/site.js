class Base64 {
    static #textEncoder = new TextEncoder();
    static #textDecoder = new TextDecoder();

    encode = (str) => btoa(String.fromCharCode(...Base64.#textEncoder.encode(str)));
    decode = (str) => Base64.#textDecoder.decode(Uint8Array.from(atob(str), c => c.charCodeAt(0)));
    encodeUrl = (str) => this.encode(str).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
    decodeUrl = (str) => this.decode(str.replace(/\-/g, '+').replace(/\_/g, '/'));

    jwtEncodeBody = (header, payload) => this.encodeUrl(JSON.stringify(header)) + '.' + this.encodeUrl(JSON.stringify(payload));
    jwtDecodePayload = (jwt) => JSON.parse(this.decodeUrl(jwt.split('.')[1]));
}

function setValidationState(input, isValid, message = '') {
    input.classList.remove('is-invalid', 'is-valid');
    const feedback = input.parentElement.querySelector('.invalid-feedback');

    if (isValid) {
        input.classList.add('is-valid');
        if (feedback) feedback.textContent = '';
    } else {
        input.classList.add('is-invalid');
        if (feedback) feedback.textContent = message;
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const signInForm = document.getElementById('sign-in-form');
    if (signInForm) {
        signInForm.querySelectorAll('input').forEach(input => {
            input.addEventListener('input', function () {
                this.classList.remove('is-invalid', 'is-valid');
            });
        });
    }
});

document.addEventListener('submit', e => {
    if (e.target.id === 'sign-in-form') {
        e.preventDefault();

        const loginInput = e.target.querySelector('[name="user-login"]');
        const passwordInput = e.target.querySelector('[name="user-password"]');

        let hasErrors = false;
        if (!loginInput.value.trim()) {
            setValidationState(loginInput, false, 'Логін не може бути порожнім');
            hasErrors = true;
        } else {
            setValidationState(loginInput, true);
        }
        if (!passwordInput.value) {
            setValidationState(passwordInput, false, 'Пароль не може бути порожнім');
            hasErrors = true;
        } else {
            setValidationState(passwordInput, true);
        }

        if (hasErrors) return;
        fetch('/User/SignIn', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                login: loginInput.value.trim(),
                password: passwordInput.value
            })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    bootstrap.Modal.getInstance(document.getElementById('authModal')).hide();
                    e.target.reset();
                    alert('Успішний вхід!');
                    console.log("Успішний вхід!");
                } else {
                    if (data.errors?.login) {
                        setValidationState(loginInput, false, data.errors.login);
                    }
                    if (data.errors?.password) {
                        setValidationState(passwordInput, false, data.errors.password);
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Помилка підключення до сервера');
            });
    }
});