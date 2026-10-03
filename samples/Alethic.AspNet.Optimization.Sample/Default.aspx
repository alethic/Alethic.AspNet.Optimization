<%@ Page Language="C#" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>Alethic.AspNet.Optimization</title>
    <%: Styles.Render("~/bundle/site.css") %>
</head>
<body>
    <main class="page">
        <h1>Alethic.AspNet.Optimization</h1>
        <p class="lede">
            Each panel turns green when its bundle has done its job. Optimizations are
            <strong><%: BundleTable.EnableOptimizations ? "enabled" : "disabled" %></strong>:
            <%: BundleTable.EnableOptimizations
                ? "bundles are minified and rendered as one versioned URL each."
                : "bundles are rendered as themselves, unminified, with an inline source map. Open the browser's tools to see each file as written." %>
        </p>

        <section class="panel" id="style">
            <h2>Stylesheet</h2>
            <p>Sass: <code>site.scss</code> with its <code>_variables</code> and <code>_panel</code> partials.</p>
            <p class="panel__result">This panel is green when the stylesheet's nested rules and variables compiled.</p>
        </section>

        <section class="panel" id="classic">
            <h2>Classic scripts</h2>
            <p><code>greeting.js</code> declares a global function; <code>status.ts</code>, TypeScript, calls it.</p>
            <p class="panel__result">Waiting for the script&hellip;</p>
        </section>

        <section class="panel" id="module">
            <h2>Modules</h2>
            <p><code>main.ts</code> imports a class from <code>counter.ts</code>; neither leaks a global.</p>
            <p class="panel__result">Waiting for the module&hellip;</p>
            <button type="button" class="counter" disabled>Clicked 0 times</button>
        </section>

        <section class="rendered">
            <h2>What <code>Render</code> wrote</h2>
            <pre><%: Styles.Render("~/bundle/site.css").ToHtmlString().Trim() %>
<%: Scripts.Render("~/bundle/classic.js").ToHtmlString().Trim() %>
<%: Scripts.Render("~/bundle/app.js").ToHtmlString().Trim() %></pre>
        </section>
    </main>

    <%: Scripts.Render("~/bundle/classic.js") %>
    <%: Scripts.Render("~/bundle/app.js") %>
</body>
</html>
