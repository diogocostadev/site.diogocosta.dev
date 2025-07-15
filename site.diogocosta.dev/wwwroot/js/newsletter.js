// Newsletter ultra-otimizado - Performance otimizada
(function() {
    'use strict';
    
    // Cache de elementos para melhor performance
    var formCache = {};
    var messageCache = {};
    
    function init() {
        // Usar querySelectorAll mais eficiente
        var forms = document.querySelectorAll('form[id$="Form"]');
        
        // Processar formulários com forEach para melhor performance
        Array.prototype.forEach.call(forms, function(form) {
            form.addEventListener('submit', handleSubmit);
            // Cache dos elementos para evitar múltiplas consultas DOM
            formCache[form.id] = {
                form: form,
                email: form.querySelector('input[name="Email"]'),
                submitBtn: form.querySelector('button[type="submit"]')
            };
            messageCache[form.id] = document.getElementById(form.id + 'Message');
        });
        
        // Dropdown menu - otimizado
        var btn = document.getElementById('joinButton');
        var form = document.getElementById('subscribeForm');
        if (btn && form) {
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                form.classList.toggle('active');
            });
        }
    }
    
    // Validação de email otimizada
    function validateEmail(email) {
        // Regex mais simples e rápida
        return email.indexOf('@') > 0 && email.indexOf('.') > email.indexOf('@');
    }

    function showMessage(msg, text, isSuccess) {
        if (!msg) return;
        
        msg.innerHTML = text;
        msg.className = 'mt-4 alert ' + (isSuccess ? 'alert-success' : 'alert-danger');
        msg.style.display = 'block';
        
        // Usar setTimeout otimizado
        if (isSuccess) {
            setTimeout(function() {
                msg.style.display = 'none';
            }, 5000);
        }
    }

    function handleSubmit(e) {
        e.preventDefault();
        
        var formId = this.id;
        var cached = formCache[formId];
        var msg = messageCache[formId];
        
        if (!cached) return;
        
        var email = cached.email.value.trim();
        var submitButton = cached.submitBtn;
        
        // Validações rápidas
        if (!email) {
            showMessage(msg, 'Por favor, digite seu e-mail.', false);
            return;
        }
        
        if (!validateEmail(email)) {
            showMessage(msg, 'Por favor, digite um e-mail válido.', false);
            return;
        }
        
        // State do botão
        var originalText = submitButton.innerHTML;
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Enviando...';
        }
        
        // Endpoint otimizado
        var endpoint = '/' + (formId.includes('sobre') ? 'Sobre' : 'Home') + '/Newsletter';
        
        // Fetch otimizado
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
                cached.email.value = '';
            } else {
                showMessage(msg, data.message || 'Erro ao processar seu cadastro.', false);
            }
        })
        .catch(function(error) {
            console.error('Erro:', error);
            showMessage(msg, 'Erro de conexão. Tente novamente.', false);
        })
        .finally(function() {
            // Restaurar botão
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.innerHTML = originalText;
            }
        });
    }
    
    // Inicialização otimizada
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})(); 