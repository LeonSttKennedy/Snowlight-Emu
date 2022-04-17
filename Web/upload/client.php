<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

// Requires all Classes and some Security Settings
require_once 'brain.php';
// Lets Check is Sessions is Registred
$GetSecurity->Get_Session();

// Template Stuff
$GetTemplate->GetTPL("client_header");
$GetTemplate->WriteLine("
<link rel=\"shortcut icon\" href=\"http://".SITE_DOMAIN."/icon.ico\" type=\"image/x-icon\" /> 
");
$GetTemplate->WriteLine("<title>".SITE_NAME." - Client</title>");

$ssousername = $_GET['username'] != null && $GetUsers->HasRight($GetUsers->Name2Id($_SESSION['account_name']), 'hk_external_login') ? $_GET['username'] : $_SESSION['account_name'];
?>

<script type="text/javascript">
	var BaseUrl = "<?php echo CLIENT_BASE; ?>";
    var flashvars =
	{
            "client.starting" : "<?php echo CLIENT_LOADING; ?>", 
            "client.allow.cross.domain" : "<?php echo CLIENT_CROSSDOMAIN; ?>", 
            "client.notify.cross.domain" : "0", 
            "connection.info.host" : "<?php echo CLIENT_HOST; ?>", 
            "connection.info.port" : "<?php echo CLIENT_PORT; ?>", 
            "site.url" : "http://<?php echo SITE_DOMAIN; ?>", 
            "url.prefix" : "http://<?php echo SITE_DOMAIN; ?>", 
            "client.reload.url" : "http://<?php echo SITE_DOMAIN; ?>/error.php", 
            "client.fatal.error.url" : "http://<?php echo SITE_DOMAIN; ?>/error.php", 
            "client.connection.failed.url" : "http://<?php echo SITE_DOMAIN; ?>/error.php", 
            "external.variables.txt" : "http://<?php echo SITE_DOMAIN; ?>/externals.php?id=external_variables", 
            "external.texts.txt" : "http://<?php echo SITE_DOMAIN; ?>/externals.php?id=external_flash_texts", 
            "external.override.texts.txt" : BaseUrl + "/external_flash_override_texts.txt", 
            "external.override.variables.txt" : BaseUrl + "/external_override_variables.txt", 
            "productdata.load.url" : BaseUrl + "/productdata.txt", 
            "furnidata.load.url" : BaseUrl + "/furnidata.txt", 
            "use.sso.ticket" : "<?php echo CLIENT_SSOTICKET; ?>", 
            "sso.ticket" : "<?php echo $GetUsers->SSO($ssousername); ?>", 
            "processlog.enabled" : "0", 
            "flash.client.url" : BaseUrl, 
            "flash.client.origin" : "<?php echo CLIENT_ORIGIN; ?>" 
    };

    var params =
	{
        "base" : BaseUrl + "/",
        "allowScriptAccess" : "always",
        "menu" : "false"                
    };
	
	swfobject.embedSWF(BaseUrl + "/<?php echo CLIENT_SWFNAME; ?>", "client", "100%", "100%", "10.0.0", "http://<?php echo SITE_DOMAIN?>/flash/expressInstall.swf", flashvars, params, null);
</script>

<?php $GetTemplate->GetTPL("client_footer"); ?>