// Newsletter ultra-otimizado
(function() {
    'use strict';
    
    function init() {
        // Processar formulários newsletter
        var forms = document.querySelectorAll('form[id$="Form"]');
        for (var i = 0; i < forms.length; i++) {
            forms[i].addEventListener('submit', handleSubmit);
        }
        
        // Dropdown menu
        var btn = document.getElementById('joinButton');
        var form = document.getElementById('subscribeForm');
        if (btn && form) {
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                form.classList.toggle('active');
            });
        }
    }
    
    function validateEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    function showMessage(msg, text, isSuccess) {
        if (!msg) return;
        
        msg.innerHTML = text;
        msg.className = 'mt-4 alert ' + (isSuccess ? 'alert-success' : 'alert-danger');
        msg.style.display = 'block';
        
        // Ocultar mensagem após 5 segundos se for sucesso
        if (isSuccess) {
            setTimeout(function() {
                msg.style.display = 'none';
            }, 5000);
        }
    }

    function handleSubmit(e) {
        e.preventDefault();
        var form = this;
        var email = form.querySelector('input[name="Email"]').value.trim();
        var msg = document.getElementById(form.id + 'Message');
        var submitButton = form.querySelector('button[type="submit"]');
        
        // Validar email
        if (!email) {
            showMessage(msg, 'Por favor, digite seu e-mail.', false);
            return;
        }
        
        if (!validateEmail(email)) {
            showMessage(msg, 'Por favor, digite um e-mail válido.', false);
            return;
        }
        
        // Desabilitar botão
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Enviando...';
        }
        
        var endpoint = '/' + (form.id.includes('sobre') ? 'Sobre' : 'Home') + '/Newsletter';
        
        fetch(endpoint, {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: 'Email=' + encodeURIComponent(email)
        })
        .then(function(response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(function(data) {
            if (data.success) {
                showMessage(msg, data.message || 'Cadastro realizado com sucesso!', true);
                form.querySelector('input[name="Email"]').value = '';
            } else {
                showMessage(msg, data.message || 'Erro ao processar seu cadastro.', false);
            }
        })
        .catch(function(error) {
            console.error('Erro:', error);
            showMessage(msg, 'Erro de conexão. Tente novamente.', false);
        })
        .finally(function() {
            // Reabilitar botão
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-envelope me-2"></i>Inscreva-se Agora';
            }
        });
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})(); 