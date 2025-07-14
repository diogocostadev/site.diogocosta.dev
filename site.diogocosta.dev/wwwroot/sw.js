// Service Worker Otimizado para Performance e Cache
const CACHE_NAME = 'diogocosta-v2.1';
const STATIC_CACHE = 'static-v2.1';
const DYNAMIC_CACHE = 'dynamic-v2.1';

// Recursos críticos para cache imediato
const CRITICAL_ASSETS = [
    '/',
    '/css/critical.css',
    '/js/newsletter.js',
    '/img/IMG_0045.webp',
    '/img/logome.webp',
    '/manifest.json'
];

// Recursos para cache estático
const STATIC_ASSETS = [
    '/img/D.png',
    '/img/D_180.png',
    '/robots.txt',
    '/sitemap.xml'
];

// URLs para não cachear (tracking, analytics)
const NO_CACHE_URLS = [
    /\/analytics/,
    /\/gtag/,
    /\/fbevents/,
    /googletagmanager/,
    /connect\.facebook\.net/,
    /matomo/,
    /\?.*utm_/
];

// Install Event - Cache recursos críticos
self.addEventListener('install', event => {
    console.log('SW: Installing...');
    
    event.waitUntil(
        Promise.all([
            // Cache crítico
            caches.open(CACHE_NAME).then(cache => {
                console.log('SW: Caching critical assets');
                return cache.addAll(CRITICAL_ASSETS);
            }),
            // Cache estático
            caches.open(STATIC_CACHE).then(cache => {
                console.log('SW: Caching static assets');
                return cache.addAll(STATIC_ASSETS);
            })
        ]).then(() => {
            console.log('SW: Installation complete');
            return self.skipWaiting();
        })
    );
});

// Activate Event - Limpar caches antigos
self.addEventListener('activate', event => {
    console.log('SW: Activating...');
    
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames
                    .filter(cacheName => 
                        cacheName !== CACHE_NAME && 
                        cacheName !== STATIC_CACHE && 
                        cacheName !== DYNAMIC_CACHE
                    )
                    .map(cacheName => {
                        console.log('SW: Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    })
            );
        }).then(() => {
            console.log('SW: Activation complete');
            return self.clients.claim();
        })
    );
});

// Fetch Event - Estratégias de cache inteligentes
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);
    
    // Ignorar URLs que não devem ser cacheadas
    if (shouldSkipCache(url, request)) {
        return;
    }
    
    // Cache First para recursos estáticos (imagens, CSS, JS)
    if (request.destination === 'image' || 
        request.destination === 'script' || 
        request.destination === 'style' ||
        request.url.includes('/img/') ||
        request.url.includes('/js/') ||
        request.url.includes('/css/')) {
        
        event.respondWith(cacheFirstStrategy(request));
    }
    // Stale While Revalidate para páginas HTML
    else if (request.destination === 'document') {
        event.respondWith(staleWhileRevalidateStrategy(request));
    }
    // Network First para APIs e conteúdo dinâmico
    else {
        event.respondWith(networkFirstStrategy(request));
    }
});

// Cache First - Para recursos estáticos
async function cacheFirstStrategy(request) {
    try {
        const cachedResponse = await caches.match(request);
        if (cachedResponse) {
            return cachedResponse;
        }
        
        const networkResponse = await fetch(request);
        
        if (networkResponse && networkResponse.status === 200) {
            const cache = await caches.open(STATIC_CACHE);
            cache.put(request, networkResponse.clone());
        }
        
        return networkResponse;
    } catch (error) {
        console.log('SW: Cache first failed:', error);
        return new Response('Offline', { status: 503 });
    }
}

// Network First - Para conteúdo dinâmico
async function networkFirstStrategy(request) {
    try {
        const networkResponse = await fetch(request);
        
        if (networkResponse && networkResponse.status === 200) {
            const cache = await caches.open(DYNAMIC_CACHE);
            cache.put(request, networkResponse.clone());
        }
        
        return networkResponse;
    } catch (error) {
        console.log('SW: Network first failed, trying cache:', error);
        const cachedResponse = await caches.match(request);
        return cachedResponse || new Response('Offline', { status: 503 });
    }
}

// Stale While Revalidate - Para páginas
async function staleWhileRevalidateStrategy(request) {
    const cache = await caches.open(DYNAMIC_CACHE);
    const cachedResponse = await cache.match(request);
    
    const fetchPromise = fetch(request).then(networkResponse => {
        if (networkResponse && networkResponse.status === 200) {
            cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    }).catch(error => {
        console.log('SW: Stale while revalidate network failed:', error);
    });
    
    return cachedResponse || fetchPromise;
}

function shouldSkipCache(url, request) {
    // Não cachear URLs específicas
    if (NO_CACHE_URLS.some(pattern => pattern.test(url.href))) {
        return true;
    }
    
    // Não cachear métodos diferentes de GET
    if (request.method !== 'GET') {
        return true;
    }
    
    // Não cachear origens diferentes (exceto nosso domínio)
    if (url.origin !== location.origin && !url.href.includes('diogocosta.dev')) {
        return true;
    }
    
    return false;
}

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
