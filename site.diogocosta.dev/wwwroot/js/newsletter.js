// Função para lidar com mensagens de feedback
document.addEventListener('DOMContentLoaded', function() {
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
    
    // Função para processar todos os formulários de newsletter AJAX
    function processNewsletterForm(form, customEndpoint) {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formId = this.id;
            const controller = formId.includes('home') ? 'Home' : 
                               formId.includes('sobre') ? 'Sobre' : 
                               formId.includes('carreira') ? 'Carreira' :
                               formId.includes('header') ? 'Home' : 'Home';
            
            const email = this.querySelector('input[name="Email"]').value;
            const messageDiv = document.getElementById(formId + 'Message');
            
            if (!messageDiv) return;
            
            // Validar o email antes de enviar (simples verificação de formato)
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(email)) {
                messageDiv.classList.remove('hidden', 'bg-blue-100', 'border-blue-400', 'text-blue-700', 'bg-green-100', 'border-green-400', 'text-green-700');
                messageDiv.classList.add('bg-red-100', 'border', 'border-red-400', 'text-red-700', 'px-4', 'py-3', 'rounded');
                messageDiv.innerHTML = '<span class="block sm:inline">Por favor, insira um email válido.</span>';
                return;
            }
            
            // Mostrar mensagem de carregamento
            messageDiv.classList.remove('hidden');
            messageDiv.classList.add('bg-blue-100', 'border', 'border-blue-400', 'text-blue-700', 'px-4', 'py-3', 'rounded');
            messageDiv.innerHTML = '<span class="block sm:inline">Processando sua inscrição...</span>';
            
            // Dados para enviar em formato de formulário
            const formData = new FormData();
            formData.append('Email', email);
            
            // Determinar o endpoint
            const endpoint = customEndpoint || `/${controller}/Newsletter`;
            
            // Enviar requisição AJAX
            fetch(endpoint, {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams(formData)
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Mostrar mensagem de sucesso
                    messageDiv.classList.remove('bg-blue-100', 'border-blue-400', 'text-blue-700', 'bg-red-100', 'border-red-400', 'text-red-700');
                    messageDiv.classList.add('bg-green-100', 'border', 'border-green-400', 'text-green-700');
                    messageDiv.innerHTML = '<span class="block sm:inline">Seu cadastro foi realizado com sucesso!</span>';
                    
                    // Limpar o campo de email
                    this.querySelector('input[name="Email"]').value = '';
                } else {
                    // Mostrar mensagem de erro
                    messageDiv.classList.remove('bg-blue-100', 'border-blue-400', 'text-blue-700', 'bg-green-100', 'border-green-400', 'text-green-700');
                    messageDiv.classList.add('bg-red-100', 'border', 'border-red-400', 'text-red-700');
                    messageDiv.innerHTML = '<span class="block sm:inline">' + (data.message || "Houve um erro ao realizar seu cadastro.") + '</span>';
                }
            })
            .catch(error => {
                // Mostrar mensagem de erro
                messageDiv.classList.remove('bg-blue-100', 'border-blue-400', 'text-blue-700', 'bg-green-100', 'border-green-400', 'text-green-700');
                messageDiv.classList.add('bg-red-100', 'border', 'border-red-400', 'text-red-700');
                messageDiv.innerHTML = '<span class="block sm:inline">Ocorreu um erro ao processar sua inscrição. Por favor, tente novamente mais tarde.</span>';
                console.error('Erro:', error);
            });
        });
    }
    
    // Processar todos os formulários de newsletter no site
    const newsletterForms = document.querySelectorAll('form[id$="Form"]');
    
    newsletterForms.forEach(function(form) {
        processNewsletterForm(form);
    });
    
    // Expor a função globalmente para ser usada em outros scripts
    window.processNewsletterForm = processNewsletterForm;
}); 