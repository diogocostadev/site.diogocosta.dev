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
    
    function handleSubmit(e) {
        e.preventDefault();
        var email = this.querySelector('input[name="Email"]').value;
        var msg = document.getElementById(this.id + 'Message');
        
        if (!msg || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return;
        
        var endpoint = '/' + (this.id.includes('sobre') ? 'Sobre' : 'Home') + '/Newsletter';
        
        fetch(endpoint, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: 'Email=' + encodeURIComponent(email)
        })
        .then(function(r) { return r.json(); })
        .then(function(d) {
            if (d.success) {
                msg.innerHTML = 'Cadastro realizado!';
                msg.className = 'border px-4 py-3 rounded bg-green-100 border-green-400 text-green-700';
                e.target.querySelector('input[name="Email"]').value = '';
            }
        })
        .catch(function() {
            msg.innerHTML = 'Erro. Tente novamente.';
            msg.className = 'border px-4 py-3 rounded bg-red-100 border-red-400 text-red-700';
        });
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})(); 