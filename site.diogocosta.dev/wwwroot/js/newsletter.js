// Função para lidar com mensagens de feedback - Versão otimizada
(function() {
    'use strict';
    
    // Aguarda DOM estar pronto
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initNewsletter);
    } else {
        initNewsletter();
    }
    
    function initNewsletter() {
        // Otimização de lazy loading para imagens
        initLazyLoading();
        
        // Remove mensagens de feedback após 5 segundos
        setTimeout(function() {
            const feedbacks = document.querySelectorAll('.bg-green-100, .bg-red-100');
            feedbacks.forEach(function(element) {
                element.style.transition = 'opacity 0.5s ease-out';
                element.style.opacity = '0';
                setTimeout(function() {
                    element.style.display = 'none';
                }, 500);
            });
        }, 5000);
        
        // Gerenciar dropdown de inscrição no menu
        const joinButton = document.getElementById('joinButton');
        const subscribeForm = document.getElementById('subscribeForm');
        
        if (joinButton && subscribeForm) {
            joinButton.addEventListener('click', function(e) {
                e.preventDefault();
                subscribeForm.classList.toggle('active');
            });
            
            // Fechar dropdown quando clicar fora
            document.addEventListener('click', function(e) {
                if (!joinButton.contains(e.target) && !subscribeForm.contains(e.target)) {
                    subscribeForm.classList.remove('active');
                }
            });
        }
        
        // Processar todos os formulários de newsletter no site
        const newsletterForms = document.querySelectorAll('form[id$="Form"]');
        
        newsletterForms.forEach(function(form) {
            processNewsletterForm(form);
        });
        
        // Expor a função globalmente para ser usada em outros scripts
        window.processNewsletterForm = processNewsletterForm;
    }
    
    // Função para otimizar lazy loading de imagens
    function initLazyLoading() {
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.classList.add('loaded');
                        observer.unobserve(img);
                    }
                });
            }, {
                threshold: 0.1,
                rootMargin: '50px'
            });
            
            document.querySelectorAll('img[loading="lazy"]').forEach(img => {
                imageObserver.observe(img);
            });
        } else {
            // Fallback para browsers sem suporte
            document.querySelectorAll('img[loading="lazy"]').forEach(img => {
                img.classList.add('loaded');
            });
        }
    }
    
    // Função para processar todos os formulários de newsletter AJAX
    function processNewsletterForm(form, customEndpoint) {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formId = this.id;
            const controller = formId.includes('home') ? 'Home' : 
                               formId.includes('sobre') ? 'Sobre' : 
                               formId.includes('carreira') ? 'Carreira' :
                               formId.includes('header') ? 'Home' : 'Home';
            
            const emailInput = this.querySelector('input[name="Email"]');
            const email = emailInput.value;
            const messageDiv = document.getElementById(formId + 'Message');
            
            if (!messageDiv) return;
            
            // Validar o email antes de enviar (simples verificação de formato)
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(email)) {
                showMessage(messageDiv, 'error', 'Por favor, insira um email válido.');
                return;
            }
            
            // Mostrar mensagem de carregamento
            showMessage(messageDiv, 'loading', 'Processando sua inscrição...');
            
            // Dados para enviar em formato de formulário
            const formData = new FormData();
            formData.append('Email', email);
            
            // Determinar o endpoint
            const endpoint = customEndpoint || `/${controller}/Newsletter`;
            
            // Enviar requisição AJAX com fetch otimizado
            fetch(endpoint, {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams(formData)
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Mostrar mensagem de sucesso
                    showMessage(messageDiv, 'success', 'Seu cadastro foi realizado com sucesso!');
                    
                    // Limpar o campo de email
                    emailInput.value = '';
                } else {
                    // Mostrar mensagem de erro
                    showMessage(messageDiv, 'error', data.message || "Houve um erro ao realizar seu cadastro.");
                }
            })
            .catch(error => {
                // Mostrar mensagem de erro
                showMessage(messageDiv, 'error', 'Ocorreu um erro ao processar sua inscrição. Por favor, tente novamente mais tarde.');
                console.error('Erro:', error);
            });
        });
    }
    
    // Função auxiliar para mostrar mensagens otimizada
    function showMessage(messageDiv, type, message) {
        // Remover todas as classes de estilo
        messageDiv.classList.remove('hidden', 'bg-blue-100', 'border-blue-400', 'text-blue-700', 
                                   'bg-green-100', 'border-green-400', 'text-green-700',
                                   'bg-red-100', 'border-red-400', 'text-red-700');
        
        // Adicionar classes base
        messageDiv.classList.add('border', 'px-4', 'py-3', 'rounded');
        
        // Adicionar classes específicas do tipo
        switch(type) {
            case 'loading':
                messageDiv.classList.add('bg-blue-100', 'border-blue-400', 'text-blue-700');
                break;
            case 'success':
                messageDiv.classList.add('bg-green-100', 'border-green-400', 'text-green-700');
                break;
            case 'error':
                messageDiv.classList.add('bg-red-100', 'border-red-400', 'text-red-700');
                break;
        }
        
        messageDiv.innerHTML = '<span class="block sm:inline">' + message + '</span>';
    }
})(); 