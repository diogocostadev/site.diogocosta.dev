// Otimização de Imagens - WebP com fallback e lazy loading
document.addEventListener('DOMContentLoaded', function() {
    initImageOptimization();
});

function initImageOptimization() {
    // Verificar suporte a WebP
    checkWebPSupport().then(supportsWebP => {
        document.documentElement.classList.add(supportsWebP ? 'webp' : 'no-webp');
        
        // Inicializar lazy loading
        if ('IntersectionObserver' in window) {
            initLazyLoading();
        } else {
            // Fallback para navegadores sem IntersectionObserver
            loadAllImages();
        }
    });
}

// Verificar suporte a WebP
function checkWebPSupport() {
    return new Promise(resolve => {
        const webP = new Image();
        webP.onload = webP.onerror = function () {
            resolve(webP.height === 2);
        };
        webP.src = 'data:image/webp;base64,UklGRjoAAABXRUJQVlA4IC4AAACyAgCdASoCAAIALmk0mk0iIiIiIgBoSygABc6WWgAA/veff/0PP8bA//LwYAAA';
    });
}

// Lazy loading com Intersection Observer
function initLazyLoading() {
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                loadImage(img);
                observer.unobserve(img);
            }
        });
    }, {
        root: null,
        rootMargin: '50px',
        threshold: 0.1
    });

    // Observar todas as imagens com data-src
    document.querySelectorAll('img[data-src]').forEach(img => {
        imageObserver.observe(img);
    });
}

// Carregar imagem otimizada
function loadImage(img) {
    const supportsWebP = document.documentElement.classList.contains('webp');
    
    // Determinar fonte da imagem
    let src = img.dataset.src;
    if (supportsWebP && img.dataset.srcWebp) {
        src = img.dataset.srcWebp;
    }
    
    // Carregar imagem
    const imageLoader = new Image();
    imageLoader.onload = function() {
        img.src = src;
        img.classList.add('loaded');
        img.removeAttribute('data-src');
        img.removeAttribute('data-src-webp');
    };
    
    imageLoader.onerror = function() {
        // Fallback para imagem original
        if (img.dataset.srcFallback) {
            img.src = img.dataset.srcFallback;
        } else {
            img.src = img.dataset.src;
        }
        img.classList.add('loaded');
    };
    
    imageLoader.src = src;
}

// Fallback para navegadores sem suporte
function loadAllImages() {
    document.querySelectorAll('img[data-src]').forEach(img => {
        loadImage(img);
    });
}

// Preload de imagens críticas
function preloadCriticalImages() {
    const criticalImages = [
        '/img/IMG_0045.webp',
        '/img/logome.webp'
    ];
    
    criticalImages.forEach(src => {
        const link = document.createElement('link');
        link.rel = 'preload';
        link.as = 'image';
        link.href = src;
        document.head.appendChild(link);
    });
}

// Redimensionamento responsivo automático
function createResponsiveImage(src, alt, sizes = '(max-width: 768px) 100vw, 50vw') {
    const img = document.createElement('img');
    
    // Gerar srcset para diferentes tamanhos
    const baseName = src.replace(/\.[^/.]+$/, '');
    const extension = src.split('.').pop();
    
    const srcset = [
        `${baseName}_400w.${extension} 400w`,
        `${baseName}_800w.${extension} 800w`,
        `${baseName}_1200w.${extension} 1200w`
    ].join(', ');
    
    img.dataset.src = src;
    img.dataset.srcset = srcset;
    img.alt = alt;
    img.sizes = sizes;
    img.loading = 'lazy';
    img.decoding = 'async';
    
    // Placeholder para evitar layout shift
    img.style.backgroundColor = '#f3f4f6';
    img.style.minHeight = '200px';
    
    return img;
}

// Otimização de imagens específicas do site
function optimizeSiteImages() {
    // Otimizar imagem de perfil
    const profileImg = document.querySelector('img[alt*="Diogo Costa"]');
    if (profileImg && !profileImg.dataset.optimized) {
        profileImg.dataset.srcWebp = '/img/IMG_0045.webp';
        profileImg.dataset.srcFallback = '/img/IMG_0045.jpg';
        profileImg.dataset.optimized = 'true';
        profileImg.loading = 'eager'; // Crítica para above-the-fold
    }
    
    // Otimizar logo
    const logoImg = document.querySelector('img[src*="logome"]');
    if (logoImg && !logoImg.dataset.optimized) {
        logoImg.dataset.srcWebp = '/img/logome.webp';
        logoImg.dataset.srcFallback = '/img/logome.png';
        logoImg.dataset.optimized = 'true';
        logoImg.loading = 'eager'; // Crítico para navegação
    }
}

// Executar otimizações
document.addEventListener('DOMContentLoaded', function() {
    optimizeSiteImages();
    preloadCriticalImages();
});

// Monitor de performance de imagens
function monitorImagePerformance() {
    if ('PerformanceObserver' in window) {
        const observer = new PerformanceObserver((list) => {
            list.getEntries().forEach((entry) => {
                if (entry.initiatorType === 'img') {
                    console.log(`Imagem carregada: ${entry.name} em ${entry.duration}ms`);
                }
            });
        });
        
        observer.observe({ entryTypes: ['resource'] });
    }
}

// Export para uso em outros scripts
window.ImageOptimizer = {
    checkWebPSupport,
    loadImage,
    createResponsiveImage,
    preloadCriticalImages,
    monitorImagePerformance
};
