(function () {
    'use strict';

    function getFlag(name) {
        return document.body?.dataset?.[name] === 'true';
    }

    function initThemeToggle() {
        const themeToggle = document.getElementById('themeToggle');
        const html = document.documentElement;
        const icon = themeToggle?.querySelector('i');
        const brandLogo = document.querySelector('.brand-logo-img');

        if (!themeToggle || !icon) {
            return;
        }

        function applyTheme(theme) {
            const isDark = theme === 'dark';

            if (theme === 'dark') {
                html.setAttribute('data-theme', 'dark');
                icon.classList.remove('fa-moon');
                icon.classList.add('fa-sun');
            } else {
                html.removeAttribute('data-theme');
                icon.classList.remove('fa-sun');
                icon.classList.add('fa-moon');
            }

            if (brandLogo) {
                const logoSrc = isDark
                    ? brandLogo.dataset.darkLogo
                    : brandLogo.dataset.lightLogo;

                if (logoSrc) {
                    brandLogo.src = logoSrc;
                }
            }
        }

        applyTheme(localStorage.getItem('theme') || 'light');

        themeToggle.addEventListener('click', function () {
            const currentTheme = html.getAttribute('data-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

            applyTheme(newTheme);
            localStorage.setItem('theme', newTheme);

            icon.style.transform = 'rotate(360deg)';
            window.setTimeout(function () {
                icon.style.transform = 'rotate(0deg)';
            }, 300);
        });
    }

    function initSidebarActions() {
        const themeToggle = document.getElementById('themeToggle');

        if (!themeToggle || document.querySelector('.notification-action')) {
            return;
        }

        let actions = themeToggle.closest('.sidebar-actions');

        if (!actions) {
            actions = document.createElement('div');
            actions.className = 'sidebar-actions';
            const sidebarNav = document.querySelector('.sidebar-nav');

            if (sidebarNav) {
                sidebarNav.appendChild(actions);
            } else {
                themeToggle.parentNode?.insertBefore(actions, themeToggle);
            }

            actions.appendChild(themeToggle);
        }

        themeToggle.classList.add('sidebar-action', 'nav-theme-toggle');

        const notificationLink = document.createElement('a');
        notificationLink.href = '/NotificationSettings';
        notificationLink.className = 'sidebar-action notification-action';
        notificationLink.title = 'Bildirim Ayarları';
        notificationLink.setAttribute('aria-label', 'Bildirim Ayarları');
        notificationLink.innerHTML = '<svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M18 10.5V9a6 6 0 0 0-12 0v1.5c0 2.1-.7 3.4-1.6 4.5A1.2 1.2 0 0 0 5.3 17h13.4a1.2 1.2 0 0 0 .9-2c-.9-1.1-1.6-2.4-1.6-4.5Z" /><path d="M9.8 19a2.4 2.4 0 0 0 4.4 0" /></svg>';

        actions.insertBefore(notificationLink, themeToggle);
    }

    function initMobileMenu() {
        const body = document.body;
        const sidebar = document.getElementById('appSidebar');
        const toggle = document.getElementById('mobileMenuToggle');
        const closeButton = document.getElementById('mobileMenuClose');
        const backdrop = document.getElementById('mobileMenuBackdrop');

        if (!body || !sidebar || !toggle || !backdrop) {
            return;
        }

        const focusableSelector = 'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';
        let lastFocusedElement = null;

        function setMenuState(isOpen) {
            body.classList.toggle('mobile-menu-open', isOpen);
            toggle.setAttribute('aria-expanded', String(isOpen));
            toggle.setAttribute('aria-label', isOpen ? 'Menüyü kapat' : 'Menüyü aç');
            backdrop.hidden = !isOpen;

            if (isOpen) {
                lastFocusedElement = document.activeElement;
                window.setTimeout(function () {
                    const firstFocusable = sidebar.querySelector(focusableSelector);
                    firstFocusable?.focus();
                }, 120);
                return;
            }

            if (lastFocusedElement instanceof HTMLElement) {
                lastFocusedElement.focus();
            }
        }

        function closeMenu() {
            setMenuState(false);
        }

        toggle.addEventListener('click', function () {
            setMenuState(!body.classList.contains('mobile-menu-open'));
        });

        closeButton?.addEventListener('click', closeMenu);
        backdrop.addEventListener('click', closeMenu);

        sidebar.querySelectorAll('a[href]').forEach(function (link) {
            link.addEventListener('click', closeMenu);
        });

        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape' && body.classList.contains('mobile-menu-open')) {
                closeMenu();
            }
        });

        window.addEventListener('resize', function () {
            if (window.innerWidth > 1024 && body.classList.contains('mobile-menu-open')) {
                closeMenu();
            }
        });
    }

    function initStockAi() {
        const chatDiv = document.getElementById('stockaiChat');
        const input = document.getElementById('stockaiInput');
        const sendBtn = document.getElementById('stockaiSend');
        const askUrl = document.body?.dataset?.stockaiAskUrl;

        if (!chatDiv || !input || !sendBtn || !askUrl) {
            return;
        }

        function getCsrfToken() {
            return document.querySelector('[name="__RequestVerificationToken"]')?.value ||
                document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
                '';
        }

        function appendMessage(className, content, asHtml) {
            const messageDiv = document.createElement('div');
            messageDiv.className = className;

            if (asHtml) {
                messageDiv.innerHTML = content;
            } else {
                messageDiv.textContent = content;
            }

            chatDiv.appendChild(messageDiv);
            chatDiv.scrollTop = chatDiv.scrollHeight;
            return messageDiv;
        }

        function appendInlineFormattedText(parent, text) {
            const parts = String(text || '').split(/(`[^`]+`)/g);

            parts.forEach(function (part) {
                if (!part) {
                    return;
                }

                if (part.startsWith('`') && part.endsWith('`') && part.length > 1) {
                    const code = document.createElement('code');
                    code.textContent = part.slice(1, -1);
                    parent.appendChild(code);
                    return;
                }

                parent.appendChild(document.createTextNode(part));
            });
        }

        function appendBotResponse(content) {
            const messageDiv = document.createElement('div');
            messageDiv.className = 'stockai-message bot stockai-formatted';

            const icon = document.createElement('i');
            icon.className = 'fas fa-robot stockai-message-icon';
            icon.setAttribute('aria-hidden', 'true');
            messageDiv.appendChild(icon);

            const body = document.createElement('div');
            body.className = 'stockai-message-content';

            const lines = String(content || 'Yanıt alınamadı.')
                .split(/\r?\n/)
                .map(function (line) { return line.trim(); })
                .filter(Boolean);

            let activeList = null;

            lines.forEach(function (line) {
                if (line.startsWith('- ')) {
                    if (!activeList) {
                        activeList = document.createElement('ul');
                        body.appendChild(activeList);
                    }

                    const item = document.createElement('li');
                    appendInlineFormattedText(item, line.slice(2));
                    activeList.appendChild(item);
                    return;
                }

                activeList = null;

                const paragraph = document.createElement('p');
                if (line.endsWith(':') && line.length <= 80) {
                    paragraph.className = 'stockai-response-heading';
                }
                appendInlineFormattedText(paragraph, line);
                body.appendChild(paragraph);
            });

            messageDiv.appendChild(body);
            chatDiv.appendChild(messageDiv);
            chatDiv.scrollTop = chatDiv.scrollHeight;
            return messageDiv;
        }

        async function sendMessage(quickPrompt) {
            const message = String(quickPrompt || input.value).trim();

            if (!message) {
                return;
            }

            appendMessage('stockai-message user', message);

            input.value = '';
            input.disabled = true;
            sendBtn.disabled = true;
            const loadingDiv = appendMessage('stockai-message bot loading', 'Yanıt hazırlanıyor');

            try {
                const response = await fetch(askUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': getCsrfToken()
                    },
                    body: JSON.stringify({ message: message })
                });

                loadingDiv.remove();

                if (response.ok) {
                    const data = await response.json();
                    appendBotResponse(data.response || 'Yanıt alınamadı.');
                    return;
                }

                let errorMessage = 'Şu anda yanıt veremiyorum. Lütfen biraz sonra tekrar deneyin.';
                try {
                    const errorData = await response.json();
                    if (errorData?.response) {
                        errorMessage = errorData.response;
                    }
                } catch {
                }

                appendMessage('stockai-message bot error', errorMessage);
            } catch {
                loadingDiv.remove();
                appendMessage('stockai-message bot error', 'Bağlantı kurulamadı. Ağ bağlantınızı kontrol edip tekrar deneyin.');
            } finally {
                input.disabled = false;
                sendBtn.disabled = false;
                input.focus();
            }
        }

        sendBtn.addEventListener('click', function () {
            sendMessage();
        });
        chatDiv.addEventListener('click', function (event) {
            const quickReply = event.target.closest('[data-stockai-quick-prompt]');

            if (!quickReply || input.disabled) {
                return;
            }

            sendMessage(quickReply.dataset.stockaiQuickPrompt);
        });

        input.addEventListener('keydown', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                sendMessage();
            }
        });

        document.getElementById('stockaiPanel')?.addEventListener('shown.bs.offcanvas', function () {
            input.focus();
        });
    }

    function showAppToast(message, type, options) {
        const toastContainer = document.getElementById('stockAlertToastContainer');

        if (!toastContainer || !window.bootstrap || !message) {
            return;
        }

        const toastType = type || 'info';
        const titleMap = {
            success: 'İşlem Başarılı',
            error: 'İşlem Tamamlanamadı',
            warning: 'Uyarı',
            info: 'Bilgi'
        };
        const iconMap = {
            success: 'fa-check',
            error: 'fa-triangle-exclamation',
            warning: 'fa-circle-exclamation',
            info: 'fa-circle-info'
        };

        const toastEl = document.createElement('div');
        toastEl.className = `toast sp-app-toast sp-app-toast-${toastType} mb-2`;
        toastEl.setAttribute('role', toastType === 'error' ? 'alert' : 'status');
        toastEl.setAttribute('aria-live', toastType === 'error' ? 'assertive' : 'polite');
        toastEl.setAttribute('aria-atomic', 'true');

        const header = document.createElement('div');
        header.className = 'sp-app-toast-header';

        const icon = document.createElement('span');
        icon.className = 'sp-app-toast-icon';
        icon.innerHTML = `<i class="fas ${iconMap[toastType] || iconMap.info}"></i>`;

        const title = document.createElement('strong');
        title.textContent = options?.title || titleMap[toastType] || titleMap.info;

        const closeButton = document.createElement('button');
        closeButton.type = 'button';
        closeButton.className = 'btn-close';
        closeButton.setAttribute('data-bs-dismiss', 'toast');
        closeButton.setAttribute('aria-label', 'Kapat');

        const body = document.createElement('div');
        body.className = 'sp-app-toast-body';
        body.textContent = message;

        header.appendChild(icon);
        header.appendChild(title);
        header.appendChild(closeButton);
        toastEl.appendChild(header);
        toastEl.appendChild(body);
        toastContainer.appendChild(toastEl);

        const toastInstance = new bootstrap.Toast(toastEl, { delay: options?.delay || 4800, autohide: true });
        toastEl.addEventListener('hidden.bs.toast', function () {
            toastEl.remove();
        });
        toastInstance.show();
    }

    function initAppToasts() {
        window.StockifyToast = showAppToast;

        const payload = document.getElementById('appToastPayload');

        if (!payload) {
            return;
        }

        const successMessage = payload.dataset.successMessage;
        const errorMessage = payload.dataset.errorMessage;

        if (successMessage) {
            showAppToast(successMessage, 'success');
        }

        if (errorMessage) {
            showAppToast(errorMessage, 'error');
        }
    }

    function initStockAlerts() {
        if (!getFlag('isSignedIn') || !window.signalR || !window.bootstrap) {
            return;
        }

        const toastContainer = document.getElementById('stockAlertToastContainer');
        const hubUrl = document.body?.dataset?.notificationHubUrl || '/hubs/notification';

        if (!toastContainer) {
            return;
        }

        function showStockAlertToast(payload) {
            const productName = payload?.productName ?? payload?.ProductName ?? 'Bilinmeyen Urun';
            const remainingStock = payload?.remainingStock ?? payload?.RemainingStock ?? '-';
            const criticalLevel = payload?.criticalLevel ?? payload?.CriticalLevel ?? '-';
            const alertTime = payload?.alertTime ?? payload?.AlertTime ?? new Date().toLocaleTimeString('tr-TR');

            const toastEl = document.createElement('div');
            toastEl.className = 'toast stock-alert-toast mb-2';
            toastEl.setAttribute('role', 'alert');
            toastEl.setAttribute('aria-live', 'assertive');
            toastEl.setAttribute('aria-atomic', 'true');

            toastEl.innerHTML = `
                <div class="toast-header">
                    <i class="fas fa-triangle-exclamation text-warning me-2"></i>
                    <strong class="me-auto">Kritik Stok Uyarisi</strong>
                    <small>${alertTime}</small>
                    <button type="button" class="btn-close ms-2 mb-1" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    <div><strong>${productName}</strong> kritik seviyeye dustu.</div>
                    <div class="small mt-1">Kalan Stok: <strong>${remainingStock}</strong> | Kritik Seviye: <strong>${criticalLevel}</strong></div>
                </div>`;

            toastContainer.appendChild(toastEl);
            const toastInstance = new bootstrap.Toast(toastEl, { delay: 4800, autohide: true });
            toastEl.addEventListener('hidden.bs.toast', function () {
                toastEl.remove();
            });
            toastInstance.show();
        }

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect()
            .build();

        connection.on('ReceiveStockAlert', function (payload) {
            if (getFlag('isPushEnabled')) {
                showStockAlertToast(payload);
            }
        });

        async function startConnection() {
            try {
                await connection.start();
            } catch {
                window.setTimeout(startConnection, 5000);
            }
        }

        startConnection();
    }

    document.addEventListener('DOMContentLoaded', function () {
        initSidebarActions();
        initMobileMenu();
        initThemeToggle();
        initStockAi();
        initAppToasts();
        initStockAlerts();
    });
})();
