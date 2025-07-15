// Service Worker otimizado para performance
const CACHE_NAME = 'diogocosta-v1.1.3';
const STATIC_CACHE_NAME = 'static-v1.1.3';
const DYNAMIC_CACHE_NAME = 'dynamic-v1.1.3';

// Recursos críticos para cache
const STATIC_RESOURCES = [
    '/',
    '/css/site.css',
    '/js/newsletter.js',
    '/img/D.png',
    '/img/D_180.png',
    '/img/logome.webp',
    '/manifest.json'
];

// Recursos externos para cache
const EXTERNAL_RESOURCES = [
    'https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css',
    'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css',
    'https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap'
];

// Instalar service worker
self.addEventListener('install', event => {
    event.waitUntil(
        Promise.all([
            caches.open(STATIC_CACHE_NAME).then(cache => {
                return cache.addAll(STATIC_RESOURCES);
            }),
            caches.open(DYNAMIC_CACHE_NAME).then(cache => {
                return cache.addAll(EXTERNAL_RESOURCES);
            })
        ])
    );
    self.skipWaiting();
});

// Ativar service worker
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== STATIC_CACHE_NAME && 
                        cacheName !== DYNAMIC_CACHE_NAME &&
                        cacheName !== CACHE_NAME) {
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
    self.clients.claim();
});

// Interceptar requests
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);
    
    // Estratégia cache-first para recursos estáticos
    if (request.destination === 'style' || 
        request.destination === 'script' || 
        request.destination === 'font' ||
        request.destination === 'image' ||
        url.pathname.startsWith('/css/') ||
        url.pathname.startsWith('/js/') ||
        url.pathname.startsWith('/img/')) {
        
        event.respondWith(
            caches.match(request).then(cachedResponse => {
                if (cachedResponse) {
                    return cachedResponse;
                }
                
                return fetch(request).then(response => {
                    if (response.status === 200) {
                        const responseClone = response.clone();
                        caches.open(STATIC_CACHE_NAME).then(cache => {
                            cache.put(request, responseClone);
                        });
                    }
                    return response;
                });
            })
        );
        return;
    }
    
    // Estratégia network-first para páginas HTML
    if (request.destination === 'document' || 
        request.headers.get('Accept')?.includes('text/html')) {
        
        event.respondWith(
            fetch(request).then(response => {
                if (response.status === 200) {
                    const responseClone = response.clone();
                    caches.open(DYNAMIC_CACHE_NAME).then(cache => {
                        cache.put(request, responseClone);
                    });
                }
                return response;
            }).catch(() => {
                return caches.match(request).then(cachedResponse => {
                    return cachedResponse || caches.match('/');
                });
            })
        );
        return;
    }
    
    // Estratégia network-first para APIs
    if (url.pathname.includes('/api/') || 
        url.pathname.includes('/Newsletter') ||
        url.pathname.includes('/Leads')) {
        
        event.respondWith(
            fetch(request).catch(() => {
                return new Response(JSON.stringify({ 
                    error: 'Offline', 
                    message: 'Sem conexão com a internet' 
                }), {
                    status: 503,
                    headers: { 'Content-Type': 'application/json' }
                });
            })
        );
        return;
    }
    
    // Default: pass through
    event.respondWith(fetch(request));
});

// Limpeza periódica do cache
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
    
    if (event.data && event.data.type === 'CLEAN_CACHE') {
        caches.keys().then(cacheNames => {
            cacheNames.forEach(cacheName => {
                if (cacheName.startsWith('dynamic-')) {
                    caches.delete(cacheName);
                }
            });
        });
    }
});

// Preload de recursos críticos
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'PRELOAD_RESOURCES') {
        const resources = event.data.resources || [];
        caches.open(STATIC_CACHE_NAME).then(cache => {
            resources.forEach(resource => {
                cache.add(resource).catch(err => {
                    console.log('Erro ao fazer preload:', resource, err);
                });
            });
        });
    }
});
