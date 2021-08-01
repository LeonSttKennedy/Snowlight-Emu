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

$PageAction = $_GET["pa"];
$error = "";
if($PageAction == "1") // Insert
{
	$user = $_POST['user'];
	$title = $_POST['title'];
	$short = urldecode($_POST['short']);
	$long = urldecode($_POST['long']);
	$mainimage = $_POST['mainimage'];
	$tempo = time();
	
	mysql_query("INSERT INTO articles (id, title, shortstory, longstory, published, author, image) VALUES (NULL, '".$title."', '".$short."', '".$long."', ".$tempo.", ".$user.", '".$mainimage."')");
	header("Location: news.php");
} 
elseif($PageAction == "2") // Edit
{
	$user = $_POST['user'];
	$title = $_POST['title'];
	$short = urldecode($_POST['short']);
	$long = urldecode($_POST['long']);
	$mainimage = $_POST['mainimage'];
	$tempo = time();
	
	mysql_query("UPDATE articles SET title = '".$title."', shortstory = '".$short."', longstory = '".$long."', image = '".$mainimage."' WHERE id ='".$_POST['id']."'");
	header("Location: news.php");
}
elseif($PageAction == "delete") // Delete
{
	mysql_query("DELETE FROM articles WHERE id ='".$_GET['id']."'");
	header("Location: news.php");
}
?>
<?php require_once("../requires/tpl/header.tpl"); ?>

<title><?php echo SITE_NAME.' - Housekeeping'; ?></title>
<body>
<script type="text/javascript" src="http://code.jquery.com/jquery-latest.min.js"></script>
<script type="text/javascript" src="includes/jquery.tooltip.js"></script>
<script type="stylesheet" href="includes/style.css"></script>
<script type="text/javascript" src="tiny_mce/jquery.tinymce.js"></script>
<!-- TinyMCE -->

<script type="text/javascript">
	$(document).ready(function(){
	$('textarea.wysiwyg').tinymce({
		script_url : '../Public/tiny_mce/tiny_mce.js',
        theme : "advanced",
		theme_advanced_buttons1 : "bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,|,fontselect,fontsizeselect",
        theme_advanced_buttons2 : "cut,copy,paste,|,undo,redo,|,link,unlink,anchor,image,cleanup,help,code,|,forecolor,backcolor",
        theme_advanced_buttons3 : "",
        skin : "o2k7",
        skin_variant : "blue",
	});
	
	$('#mainimage').keypress(function(e){
		if(e.which == 13){
			$('#ImagePreview').attr('src', $(this).val());
		}
	});
			
	$('#mainimage').change(function(){
		$('#ImagePreview').attr('src', $(this).val());
	});
	
	});
	
</script>
<!-- /TinyMCE -->

<style type="text/css">
.charentry { 
width: 115%; 
background-color: #fff; 
border-radius: 4px; 
border: 2px solid #F2F2F2; 
margin-bottom: 5px; 
margin-left: -50px;
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

<?php
$newsid = $_GET["id"];
$newsq = mysql_query("SELECT * FROM articles WHERE id = '".$newsid."' LIMIT 1");
$news = mysql_fetch_array($newsq);
if($newsid == 0)
{
?>
<form method="POST" action="./news.php?pa=1">
<h3>Publish News</h3>
<p>Post a news article</p>
<?php
$query = mysql_query("SELECT * FROM characters WHERE username = '".$_SESSION['account_name']."'");
$getinfo = mysql_fetch_assoc($query);
$id = $getinfo['id'];
?>
	<input type="hidden" name="user" id="user" value="<?php echo $id; ?>" />
	Article title:<br />
	<input type="text" name="title" id="title" style="width:300px;"/><br /><br />
	Frontpage teaser text:<br />
	<textarea class="wysiwyg" name="short" id="short" style="width:350px; height:175px"></textarea><br /><br />
	News text:<br />
	<textarea class="wysiwyg" name="long" id="long" style="width:550px; height:275px"></textarea><br /><br />
		<img src="./images/block_32.png" id="ImagePreview" /><br /><br />
			<label for="mainimage">Topstory image: </label>
			<select name="mainimage" id="mainimage" style="width:300px">
			<option value="./images/block_32.png"></option>
            <?php
            $newsimages = opendir('./images/news/') or die('Error');  
			while($images = @readdir($newsimages)) 
			{  
				$ext = pathinfo($images, PATHINFO_EXTENSION);
				if(!is_dir($images) && ($ext == "png" || $ext == "gif"))
  				echo '<option value="./images/news/'.$images.'">'.$images.'</option>';  
			}
			closedir($newsimages);  
			?>
			</select><br /><br />
	<input type="submit" class="btn" value="Submit"><br /><br />
</form>
<?php
}
else
{
?>
<form method="POST" action="./news.php?pa=2">
<h3>Edit a news story</h3>
<p>Edit an posted news article</p>
<?php
$query = mysql_query("SELECT * FROM characters WHERE username = '".$_SESSION['account_name']."'");
$getinfo = mysql_fetch_assoc($query);
$id = $getinfo['id'];
?>
	<input type="hidden" name="id" id="id" value="<?php echo $newsid; ?>" />
	Article title:<br />
	<input type="text" name="title" id="title" value="<?php echo $news['title']; ?>" style="width:300px;"/><br /><br />
	Frontpage teaser text:<br />
	<textarea class="wysiwyg" name="short" id="short" style="width:350px; height:175px"><?php echo $news['shortstory']; ?></textarea><br /><br />
	News text:<br />
	<textarea class="wysiwyg" name="long" id="long" style="width:550px; height:275px"><?php echo $news['longstory']; ?></textarea><br /><br />
			<img src="../<?php echo $news['image']; ?>" id="ImagePreview" /><br /><br />
			<label for="mainimage">Topstory image: </label>
			<select name="mainimage" id="mainimage" style="width:300px">
            <?php
            $newsimages = opendir('./images/news/') or die('Error');  
			while($images = @readdir($newsimages)) 
			{  
				$ext = pathinfo($images, PATHINFO_EXTENSION);
				if(!is_dir($images) && ($ext == "png" || $ext == "gif"))
				{
					if(preg_match("/".$images."/", $news['image']))
					$selected = 'selected="selected"';
					else
					$selected = '';
  					echo '<option value="./images/news/'.$images.'" '.$selected.'>'.$images.'</option>';  
				}
			}
			closedir($newsimages);  
			?></select><br /><br />
	<input type="submit" class="btn" value="Update"><br /><br />
</form>
<?php
}
?>
</div>

<div class="column2" id="column2">
<div class="charentry">
<div class="inner">
<div class="left" style="width: 65%;">
<b>News list</b><br />
Click on a title to edit a story<br />
<?php
$query = mysql_query("SELECT * FROM articles ORDER BY id DESC LIMIT 1;");
$num = mysql_num_rows($query);
if($num > 0)
{
?>
<div id="newsblock">

	<div id="newslist">
		<ul class="newslist-inner">
		<?php
		$query = mysql_query("SELECT * FROM articles ORDER BY id DESC LIMIT 25");
			while($news = mysql_fetch_assoc($query))
			{
				if($color == 'n')
				$color='alt';
				else
				$color='n';
				if($_GET['id'] == $news['id'])
				{
				echo'<li class="'.$color.'">
						<a><a href="http://'.SITE_DOMAIN.'/housekeeping/news.php?id='.$news['id'].'"><b>'.$news['title'].'</b></a>&nbsp;&nbsp;<a href="?pa=delete&id='.$news['id'].'" title="Deletar">(X)</a><br />
						<small>'.@date("D, d M Y h:m:s", $news['published']).'</small><br />
					</li>';
				}
				else
				{
				echo'<li class="'.$color.'">
						<a><a href="http://'.SITE_DOMAIN.'/housekeeping/news.php?id='.$news['id'].'">'.$news['title'].'</a>&nbsp;&nbsp;<a href="?pa=delete&id='.$news['id'].'" title="Deletar">(X)</a><br />
						<small>'.@date("D, d M Y h:m:s", $news['published']).'</small><br />
					</li>';
				}
			}
?>
		</ul>

	</div>

</div>
<?php
}
?>
</div>
<div class="right" style="margin-top: 20px; width: 35%; font-size: 110%;">

</div>
<div class="clear">
</div>
</div>
</div>
<div class="charentry">
<div class="inner">
<div class="left" style="width: 65%;">
<b>Comments:</b><br />
<div class="left" style="margin-top: 20px; width: 155%; font-size: 110%;">
You can use in a news variable like: { }, # #, % %<br /><br />
Example:<br />
- %username%, #username# ou {username}. This will show the name of the user who is reading the news.
</div>
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