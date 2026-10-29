(() => {
    const panel = document.getElementById('aiChatPanel');
    const launcher = document.getElementById('aiChatLauncher');
    const close = document.getElementById('aiChatClose');
    const form = document.getElementById('aiChatForm');
    const input = document.getElementById('aiChatInput');
    const messages = document.getElementById('aiChatMessages');
    const sendButton = document.getElementById('aiChatSend');
    const suggestions = document.getElementById('aiChatSuggestions');
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    if (!panel || !launcher || !close || !form || !input || !messages || !sendButton || !token) {
        return;
    }

    let historyLoaded = false;

    function addBubble(text, type) {
        messages.querySelector('.ai-chat-empty')?.remove();
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
        let result;
        try {
            result = await response.json();
        } catch {
            throw new Error('Máy chủ trả về phản hồi không hợp lệ. Vui lòng thử lại sau.');
        }
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
            const conversations = result.data || [];
            if (conversations.length) {
                suggestions?.classList.add('d-none');
            }
            for (const conversation of conversations.slice().reverse()) {
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

    async function sendMessage(message) {
        if (!message || sendButton.disabled) return;
        sendButton.disabled = true;
        suggestions?.classList.add('d-none');
        addBubble(message, 'user');
        input.value = '';
        try {
            const result = await request('/api/client/ai-chat/messages', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message })
            });
            addBubble(result.data.reply, 'assistant');
        } catch (error) {
            addBubble(error.message, 'error');
            input.value = message;
        } finally {
            sendButton.disabled = false;
            input.focus();
        }
    }

    form.addEventListener('submit', event => {
        event.preventDefault();
        sendMessage(input.value.trim());
    });

    suggestions?.addEventListener('click', event => {
        const button = event.target.closest('button[data-chat-question]');
        if (button) {
            sendMessage(button.dataset.chatQuestion);
        }
    });

    input.addEventListener('keydown', event => {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            form.requestSubmit();
        }
    });
})();
