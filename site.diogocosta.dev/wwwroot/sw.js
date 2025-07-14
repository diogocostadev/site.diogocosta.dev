const CACHE_NAME = 'site-diogocosta-v1.1';
const urlsToCache = [
    '/',
    '/js/newsletter.js',
    '/css/critical.css',
    '/img/logome.webp',
    '/img/D.png',
    '/img/D_180.png',
    '/img/IMG_0045.webp',
    '/manifest.json',
    'https://fonts.googleapis.com/css2?family=Newsreader:wght@400;500;600&display=swap',
    'https://fonts.gstatic.com/s/newsreader/v20/cY9qfjOCX1hbuyalUrK49dLac06G1ZGsZBtoBCzBDXXD9JVF.woff2'
];

// Instalar Service Worker
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('Cache aberto');
                return cache.addAll(urlsToCache);
            })
            .catch(error => {
                console.log('Erro ao adicionar recursos ao cache:', error);
            })
    );
});

// Fetch com estratégia Cache First para recursos estáticos
self.addEventListener('fetch', event => {
    // Verificar se é um recurso que deve ser cacheado
    if (event.request.destination === 'image' || 
        event.request.destination === 'script' || 
        event.request.destination === 'style' ||
        event.request.url.includes('/img/') ||
        event.request.url.includes('/js/') ||
        event.request.url.includes('fonts.googleapis.com')) {
        
        event.respondWith(
            caches.match(event.request)
                .then(response => {
                    // Cache hit - retornar resposta do cache
                    if (response) {
                        return response;
                    }
                    
                    // Fazer fetch e adicionar ao cache
                    return fetch(event.request).then(response => {
                        // Verificar se a resposta é válida
                        if (!response || response.status !== 200 || response.type !== 'basic') {
                            return response;
                        }
                        
                        // Clonar a resposta
                        const responseToCache = response.clone();
                        
                        caches.open(CACHE_NAME)
                            .then(cache => {
                                cache.put(event.request, responseToCache);
                            });
                        
                        return response;
                    });
                })
                .catch(() => {
                    // Fallback para recursos essenciais
                    if (event.request.destination === 'image') {
                        return new Response('<!-- Imagem não disponível -->', {
                            headers: { 'Content-Type': 'text/html' }
                        });
                    }
                })
        );
    }
});

// Ativar Service Worker e limpar caches antigos
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        console.log('Removendo cache antigo:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

// Background sync para melhorar a experiência offline
self.addEventListener('sync', event => {
    if (event.tag === 'newsletter-sync') {
        event.waitUntil(syncNewsletterForms());
    }
});

// Função para sincronizar formulários offline
function syncNewsletterForms() {
    return new Promise((resolve, reject) => {
        // Implementar lógica de sincronização de formulários
        // quando a conexão for restaurada
        resolve();
    });
}

// Push notifications (para futuras implementações)
self.addEventListener('push', event => {
    if (event.data) {
        const data = event.data.json();
        const options = {
            body: data.body,
            icon: '/img/D_180.png',
            badge: '/img/D.png',
            data: data.url
        };
        
        event.waitUntil(
            self.registration.showNotification(data.title, options)
        );
    }
});

// Clique em notificação
self.addEventListener('notificationclick', event => {
    event.notification.close();
    
    if (event.notification.data) {
        event.waitUntil(
            clients.openWindow(event.notification.data)
        );
    }
});
