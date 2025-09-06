document.addEventListener("submit", e => {
    const form = e.target;
    if (form.id == "product-add-form") {
        e.preventDefault();
        handleAddProduct(form);
    }
    if (form.id == "group-add-form") {
        e.preventDefault();
        handleAddProduct(form);
    }
});

function handleAddProduct(form) {
    fetch(form.action, {
        method: 'POST',
        body: new FormData(form)
    }).then(r => r.json()).then(data => {
            alert(data.name);
            if (data.status === 201) { form.reset();}
        })
        .catch(err => {console.error(err);
            alert("Помилка");
        });
}

function handleAddGroup(form) {
    fetch(form.action, {
        method: 'POST',
        body: new FormData(form)
    }).then(r => r.json()).then(console.log).catch(console.error);
}

document.addEventListener('DOMContentLoaded', e => {
    for (let btn of document.querySelectorAll("[data-product-id]")) {
        btn.addEventListener('click', addToCartClick);
    }
});

function addToCartClick(e) {
    const btn = e.target.closest("[data-product-id]");
    if (!btn) throw "closest failed '[data-product-id]'";
    const productId = btn.getAttribute("data-product-id");

    fetch(`/api/cart/${productId}`, { method: "POST" })
        .then(async response => {
            if (response.ok) {
                const result = await response.json();
                console.log("Додано до кошика, статус OK", result);
                const alert = document.createElement("div");
                alert.className = "alert alert-dark alert-dismissible fade show";
                alert.style.cssText = "position: fixed; top: 10px; left: 50%; transform: translateX(-50%); z-index: 1000; min-width: 300px;";
                alert.innerHTML = `
                    Товар додано до кошика!
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                `;
                document.body.appendChild(alert);
                setTimeout(() => alert.remove(), 3000);
            } else if (response.status === 401) {
                console.log("401 Unauthorized");
                const alert = document.createElement("div");
                alert.className = "alert alert-dark alert-dismissible fade show";
                alert.style.cssText = "position: fixed; top: 10px; left: 50%; transform: translateX(-50%); z-index: 1000; min-width: 300px;";
                alert.innerHTML = `
                    Треба увійти для додавання до кошика.
                    <button type="button" class="btn btn-dark btn-sm ms-2" onclick="new bootstrap.Modal(document.getElementById('authModal')).show()">Авторизуватися?</button>
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                `;
                document.body.appendChild(alert);
            } else {
                console.log("Error status: " + response.status);
                const alert = document.createElement("div");
                alert.className = "alert alert-danger alert-dismissible fade show";
                alert.style.cssText = "position: fixed; top: 10px; left: 50%; transform: translateX(-50%); z-index: 1000; min-width: 300px;";
                alert.innerHTML = `
                    Сталася помилка, перезавантажити сторінку?
                    <button type="button" class="btn btn-secondary btn-sm ms-2" onclick="this.parentElement.remove()">Ні</button>
                    <button type="button" class="btn btn-dark btn-sm ms-2" onclick="window.location.reload()">Так</button>
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                `;
                document.body.appendChild(alert);
            }
        })
        .catch(error => {
            console.log("Fetch error");
            console.error(error);
            const alert = document.createElement("div");
            alert.className = "alert alert-danger alert-dismissible fade show";
            alert.style.cssText = "position: fixed; top: 10px; left: 50%; transform: translateX(-50%); z-index: 1000; min-width: 300px;";
            alert.innerHTML = `
                Сталася помилка, перезавантажити сторінку?
                <button type="button" class="btn btn-secondary btn-sm ms-2" onclick="this.parentElement.remove()">Ні</button>
                <button type="button" class="btn btn-dark btn-sm ms-2" onclick="window.location.reload()">Так</button>
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;
            document.body.appendChild(alert);
        });
}