<#-- This bypasses the default Keycloak layout to give you full screen control -->
<!DOCTYPE html>
<html>
<head>
    <title>${msg("loginTitle", (realm.displayName!''))}</title>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link rel="stylesheet" href="${url.resourcesPath}/css/styles.css">
</head>
<body>

<section class="wasel-section">
    <div class="wasel-form-side">
        <div class="wasel-form-wrapper">
            
            <div class="mobile-logo">
                <img src="${url.resourcesPath}/img/wasel-logo.png" alt="Wasel Logo" />
            </div>

            <#if message?has_content && (message.type != 'warning')>
                <div class="kc-error">
                    ${kcSanitize(message.summary)?no_esc}
                </div>
            </#if>

            <h2 style="font-size: 1.5rem; font-weight: bold; margin-bottom: 1.5rem; text-align: center;">Log in to your account</h2>

            <form id="kc-form-login" onsubmit="login.disabled = true; return true;" action="${url.loginAction}" method="post">
                
                <div>
                    <label for="username" style="display: block; font-size: 0.875rem; font-weight: 500; margin-bottom: 0.5rem;">Email or Username</label>
                    <input tabindex="1" id="username" class="kc-input" name="username" value="${(login.username!'')}" type="text" autofocus autocomplete="off" />
                </div>

                <div>
                    <label for="password" style="display: block; font-size: 0.875rem; font-weight: 500; margin-bottom: 0.5rem;">Password</label>
                    <input tabindex="2" id="password" class="kc-input" name="password" type="password" autocomplete="off" />
                </div>

                <div style="margin-top: 1.5rem;">
                    <input tabindex="4" class="kc-button" name="login" id="kc-login" type="submit" value="Sign In"/>
                </div>
            </form>

        </div>
    </div>

    <div class="wasel-image-side">
        <div class="wasel-gradient"></div>

        <div class="wasel-text-overlay">
            <h1 class="wasel-title">Welcome to Wasel</h1>
            <p class="wasel-subtitle">
                Streamline your e-commerce logistics with Wasel, your all-in-one platform for shipping, tracking, and managing orders in Morocco.
            </p>
        </div>

        <img src="${url.resourcesPath}/img/login.jpg" alt="town image" class="wasel-bg-img" />
    </div>
</section>

</body>
</html>