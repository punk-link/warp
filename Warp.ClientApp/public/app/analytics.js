/* eslint-disable */
;(function () {
  try {
    if (window.__ANALYTICS_INIT) 
        return

    // Support multiple API config shapes/names to avoid tight coupling
    var root = window.__APP_CONFIG || window.APP_CONFIG || window.AppConfig || window.CONFIG || {}
    var analytics = root.analytics || root.Analytics || root

    var gtagId =
      analytics.gtag || analytics.gTag || analytics.GTag || analytics.googleTag || root.GTag || root.gtag || root.gTag || ''

    var ymNumRaw =
      analytics.yandexMetrikaNumber || analytics.YandexMetrikaNumber || analytics.metrika || root.YandexMetrikaNumber || root.yandexMetrikaNumber || ''

    var ymNum = ymNumRaw != null ? String(ymNumRaw).trim() : ''
    gtagId = gtagId != null ? String(gtagId).trim() : ''

    // Google gtag
    if (gtagId) {
      // Skip if already included
      var gtagSrc = 'https://www.googletagmanager.com/gtag/js?id=' + encodeURIComponent(gtagId)
      var exists = false
      for (var i = 0; i < document.scripts.length; i++) if (document.scripts[i].src === gtagSrc) 
        exists = true

      if (!exists) {
        var gtagScript = document.createElement('script')
        gtagScript.async = true
        gtagScript.src = gtagSrc
        document.head.appendChild(gtagScript)
      }

      window.dataLayer = window.dataLayer || []
      function gtag() { dataLayer.push(arguments) }
      window.gtag = window.gtag || gtag
      gtag('js', new Date())
      gtag('config', gtagId)
    }

    // Yandex.Metrika
    if (ymNum) {
      ;(function (m, e, t, r, i, k, a) {
        m[i] = m[i] || function () { (m[i].a = m[i].a || []).push(arguments) }
        m[i].l = 1 * new Date()
        for (var j = 0; j < document.scripts.length; j++) { if (document.scripts[j].src === r) { return } }
        k = e.createElement(t), a = e.getElementsByTagName(t)[0]
        k.async = 1
        k.src = r
        a.parentNode.insertBefore(k, a)
      })(window, document, 'script', 'https://mc.yandex.ru/metrika/tag.js', 'ym')

      if (window.ym) 
        window.ym(Number(ymNum), 'init', { clickmap: true, trackLinks: true, accurateTrackBounce: true })
    }

    window.__ANALYTICS_INIT = true
  } catch (_) { /* swallow */ }
})()
