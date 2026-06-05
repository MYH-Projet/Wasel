<!DOCTYPE html>
<html>
<head>
    <title>${msg("logoutConfirmTitle")}</title>
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

            <h2 style="font-size: 1.5rem; font-weight: bold; margin-bottom: 1.5rem; text-align: center;">Confirm Logout</h2>
            
            <p style="font-size: 1rem; color: #4b5563; text-align: center; margin-bottom: 2rem; line-height: 1.5;">
                Are you sure you want to log out of your Wasel account?
            </p>

            <form id="kc-logout-confirm" action="${url.logoutConfirmAction}" method="post">
                <input type="hidden" name="session_code" value="${logoutConfirm.code}">
                
                <div style="margin-top: 1.5rem;">
                    <input class="kc-button" name="confirmLogout" id="kc-logout" type="submit" value="Yes, Log Out"/>
                </div>
            </form>

            <#if !logoutConfirm.skipLink && (client.baseUrl)?has_content>
                <div style="margin-top: 1.5rem; text-align: center;">
                    <a href="${client.baseUrl}" style="color: #0f172a; text-decoration: none; font-size: 0.875rem; font-weight: 500; display: inline-flex; align-items: center; gap: 0.25rem;">
                        <span>&larr;</span> Back to Application
                    </a>
                </div>
            </#if>

        </div>
    </div>

    <div class="wasel-image-side">
        <div class="wasel-gradient"></div>

        <div class="wasel-text-overlay">
            <h1 class="wasel-title">good bay from wasel</h1>
            <p class="wasel-subtitle">
                Streamline your e-commerce logistics with Wasel, your all-in-one platform for shipping, tracking, and managing orders in Morocco.
            </p>
        </div>

        <img src="${url.resourcesPath}/img/login.jpg" alt="town image" class="wasel-bg-img" />
    </div>
</section>

</body>
</html>
