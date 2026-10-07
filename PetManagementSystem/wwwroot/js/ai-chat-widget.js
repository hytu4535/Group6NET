(() => {
    const panel = document.getElementById('aiChatPanel');
    const launcher = document.getElementById('aiChatLauncher');
    const close = document.getElementById('aiChatClose');
    const form = document.getElementById('aiChatForm');
    const input = document.getElementById('aiChatInput');
    const messages = document.getElementById('aiChatMessages');
    const sendButton = document.getElementById('aiChatSend');
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    if (!panel || !launcher || !close || !form || !input || !messages || !sendButton || !token) {
        return;
    }

    let historyLoaded = false;

    function addBubble(text, type) {
        const bubble = document.createElement('div');
        bubble.className = `ai-chat-bubble ai-chat-bubble-${type}`;
        bubble.textContent = text;
        messages.append(bubble);
        messages.scrollTop = messages.scrollHeight;
        return bubble;
    }

    function togglePanel(show) {
        panel.classList.toggle('d-none', !show);
        launcher.setAttribute('aria-expanded', String(show));
        launcher.innerHTML = show
            ? '<i class="bi bi-x-lg"></i>'
            : '<i class="bi bi-chat-dots-fill"></i>';
        if (show) {
            input.focus();
            loadHistory();
        }
    }

    async function request(url, options = {}) {
        const headers = new Headers(options.headers || {});
        if (options.method) {
            headers.set('RequestVerificationToken', token);
        }
        const response = await fetch(url, { ...options, headers, credentials: 'same-origin' });
        const result = await response.json();
        if (!response.ok || !result.success) {
            throw new Error(result.errors?.join(' ') || result.message || 'Không thể hoàn tất yêu cầu.');
        }
        return result;
    }

    async function loadHistory() {
        if (historyLoaded) return;
        historyLoaded = true;
        try {
            const result = await request('/api/client/ai-chat/conversations?page=1&pageSize=10');
            for (const conversation of (result.data || []).slice().reverse()) {
                addBubble(conversation.userMessage, 'user');
                if (conversation.aiMessage) {
                    addBubble(conversation.aiMessage, 'assistant');
                }
            }
        } catch (error) {
            addBubble(error.message, 'error');
        }
    }

    launcher.addEventListener('click', () => togglePanel(panel.classList.contains('d-none')));
    close.addEventListener('click', () => togglePanel(false));

    form.addEventListener('submit', async event => {
        event.preventDefault();
        const message = input.value.trim();
        if (!message || sendButton.disabled) return;

        sendButton.disabled = true;
        try {
            const result = await request('/api/client/ai-chat/messages', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message })
            });
            addBubble(message, 'user');
            addBubble(result.data.reply, 'assistant');
            input.value = '';
        } catch (error) {
            addBubble(error.message, 'error');
        } finally {
            sendButton.disabled = false;
            input.focus();
        }
    });

    input.addEventListener('keydown', event => {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            form.requestSubmit();
        }
    });
})();
