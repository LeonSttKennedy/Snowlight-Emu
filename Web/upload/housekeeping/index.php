<?php
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

require_once '../brain.php';

if(!$GetUsers->HasRight($_SESSION['account_name'], "hk_login"))
{
	$GetTemplate->Redirect("../me.php");
}
?>
<?php require_once("../requires/tpl/header.tpl"); ?>

<title><?php echo SITE_NAME.' - Housekeeping'; ?></title>
<body>
<style type="text/css">
.charentry { 
width: 100%; 
background-color: #fff; 
border-radius: 4px; 
border: 2px solid #F2F2F2; 
margin-bottom: 5px; 
margin-left: -10px;
}
.charentry .inner { 
padding: 4px 10px 6px 10px; 
}
</style>
<div class="wrapper">	
<div id="navi"> 
<div style="float: left; width: 70%;"> 
<ul class="navilist"> 
<li class="<?php if($current_subpage == "frontpage") { echo "selected"; } else {} ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/housekeeping/">Home</a></li> 
<li class="<?php if($current_subpage == "news") { echo "selected"; } else {} ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/housekeeping/news.php">News</a></li> 
<li ><a href="http://<?php echo SITE_DOMAIN; ?>/me.php">Go Back</a></li> 
</ul> 
</div> 
<?php include("../requires/tpl/server_status.tpl"); ?>
<div id="banner"> 
<div style="padding-top: 56px; margin-left: 480px;"> 
</div> 
</div>
<div id="main"> 

<div class="column1" id="column1">
<h3>Under construction!!!</h3>
<p>Coming soon all functionalityes will working fine!!!</p><br />
<?php include("../requires/texts/frontpage.php"); ?>
</div>

<div class="column2" id="column2">
<div class="charentry">
<div class="inner">
<div class="left" style="width: 65%;">
<strong>Logged as</strong>: <?php GetUserInfo($_SESSION['id'], "username"); ?>
<br/>
<br/>
<strong>Hotel statistics:</strong>
<br/>
<?php
$c_items = mysql_query("SELECT * FROM catalog_items");
$total = mysql_num_rows($c_items);
echo '<b>'.$total.'</b> Furnis in catalogue';
?>
<br />
<?php
$items = mysql_query("SELECT * FROM items");
$total = mysql_num_rows($items);
echo '<b>'.$total.'</b> Furnis bought';
?>
<br />
<?php
$rooms = mysql_query("SELECT * FROM rooms");
$total = mysql_num_rows($rooms);
echo '<b>'.$total.'</b> Rooms created';
?>
<br />
<?php
$bots = mysql_query("SELECT * FROM bots");
$total = mysql_num_rows($bots);
echo '<b>'.$total.'</b> Bots in hotel';
?>
<br />
<?php
$news = mysql_query("SELECT * FROM articles");
$total = mysql_num_rows($news);
echo '<b>'.$total.'</b> News';
?>
<br />
<?php
$users = mysql_query("SELECT * FROM users");
$total = mysql_num_rows($users);
echo '<b>'.$total.'</b> Registered users ';
?>
<br />
<?php
$chars = mysql_query("SELECT * FROM characters");
$total = mysql_num_rows($chars);
echo '<b>'.$total.'</b> Registered characters ';
?>
</div>
<div class="right" style="margin-top: 20px; width: 35%; font-size: 110%;">

</div>
<div class="clear">
</div>
</div>
</div>
</div>
</div>
<div class="clear">
</div> 
</div> 
</div> 
<?php require_once("../requires/tpl/footer.tpl"); ?>