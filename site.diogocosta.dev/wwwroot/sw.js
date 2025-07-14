// Service Worker ultra-otimizado v3
const CACHE_NAME = 'site-v3';
const CACHE_URLS = [
    '/css/critical.css',
    '/img/logome.webp',
    '/img/IMG_0045.webp',
    '/js/newsletter.js'
];

// Install
self.addEventListener('install', e => {
    e.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(CACHE_URLS))
            .then(() => self.skipWaiting())
    );
});

// Activate
self.addEventListener('activate', e => {
    e.waitUntil(
        caches.keys().then(keys => 
            Promise.all(keys.map(key => 
                key !== CACHE_NAME ? caches.delete(key) : null
            ))
        ).then(() => self.clients.claim())
    );
});

// Fetch
self.addEventListener('fetch', e => {
    if (e.request.method !== 'GET') return;
    
    e.respondWith(
        caches.match(e.request).then(cached => {
            if (cached) return cached;
            
            return fetch(e.request).then(response => {
                if (response.status === 200 && e.request.url.match(/\.(css|js|webp|jpg|png)$/)) {
                    const copy = response.clone();
                    caches.open(CACHE_NAME).then(cache => cache.put(e.request, copy));
                }
                return response;
            });
        })
    );
});
