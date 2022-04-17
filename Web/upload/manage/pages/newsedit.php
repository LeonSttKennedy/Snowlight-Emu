<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_sitemanagement'))
{
	exit;
}

$data = null;

if (isset($_GET['u']) && is_numeric($_GET['u']))
{
	$u = intval($_GET['u']);
	$getData = mysql_query("SELECT * FROM articles WHERE id = '" . $u . "' LIMIT 1");
	
	if (mysql_num_rows($getData) > 0)
	{
		$data = mysql_fetch_assoc($getData);
	}
}

if ($data == null)
{
	fMessage('error', 'Woops, that article does not exist.');
	header("Location: index.php?_cmd=news");
	exit;
}

if (isset($_POST['content']))
{
	$title = $_POST['title'];
	$teaser = $_POST['teaser'];
	$topstory = str_replace("../", "", $_POST['topstory']);
	$content = $_POST['content'];
	$category = intval($_POST['category']);
	
	if (strlen($title) < 1 || strlen($teaser) < 1 || strlen($content) < 1)
	{
		fMessage('error', 'Please fill in all fields.');
	}
	else
	{
		mysql_query("UPDATE articles SET title = '" . $title . "', category_id = '" . $category . "', image = '" . $topstory . "', longstory = '" . $content . "', shortstory = '" . $teaser . "' WHERE id = '" . $data['id'] . "' LIMIT 1");
		fMessage('ok', 'News article updated.');
		
		header("Location: index.php?_cmd=news");
		exit;
	}
}

foreach ($data as $key => $value)
{
	switch ($key)
	{
		case 'shortstory':
		
			$key = 'teaser';
			break;
			
		case 'image':
			
			$bits = explode('/', $value);
			$value = $bits[count($bits) - 1];
			$key = 'topstory';
			break;
			
		case 'longstory':
		
			$key = 'content';
			break;
	}

	if (!isset($_POST[$key]))
	{
		$_POST[$key] = $value;
	}
}

require_once "top.php";

?>			
<script type="text/javascript" src="http://code.jquery.com/jquery-latest.min.js"></script>
<script type="text/javascript" src="includes/jquery.tooltip.js"></script>
<script type="text/javascript" src="http://<?php echo hk_www; ?>/tiny_mce/jquery.tinymce.js"></script>
<!-- TinyMCE -->

<script type="text/javascript">
	$(document).ready(function(){
	$('textarea.wysiwyg').tinymce({
		script_url : 'http://<?php echo hk_www; ?>/tiny_mce/tiny_mce.js',
        theme : "advanced",
		theme_advanced_buttons1 : "bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,|,fontselect,fontsizeselect",
        theme_advanced_buttons2 : "cut,copy,paste,|,undo,redo,|,link,unlink,anchor,image,cleanup,help,code,|,forecolor,backcolor",
        theme_advanced_buttons3 : "",
        skin : "o2k7",
        skin_variant : "blue",
	});
	
	$('#topstory').keypress(function(e){
		if(e.which == 13){
			$('#ImagePreview').attr('src', $(this).val());
		}
	});
			
	$('#topstory').change(function(){
		$('#ImagePreview').attr('src', $(this).val());
	});
	
	});
	
</script>
<!-- /TinyMCE -->

<h1>Publish News</h1>
<form method="post">

<br />

<div style="float: left;">

<strong>Article title:</strong><br />
<input type="text" value="<?php if (isset($_POST['title'])) { echo $_POST['title']; } ?>" name="title" size="50" onkeyup="suggestSEO(this.value);" style="padding: 5px; font-size: 130%;"><br />
<br />

<strong>Category:</strong><br />
<select name="category">
<?php

$getOptions = mysql_query("SELECT * FROM articles_categories ORDER BY caption ASC");

while ($option = mysql_fetch_assoc($getOptions))
{
	echo '<option value="' . intval($option['id']) . '" ' . (($option['id'] == $data['category_id']) ? 'selected' : '') . '>' . $option['caption'] . '</option>';
}

?>
</select><br />
<br />

<strong>SEO-friendly URL:</strong><br />
<div style="border: 1px dotted; width: 300px; padding: 5px;">
http://<?php echo SITE_DOMAIN; ?>/<b><?php echo $data['id']; ?>-<?php echo $data['seo_link']; ?></b>/<br />
</div><br />

<strong>Frontpage teaser text:</strong><br />
<textarea class="wysiwyg" name="teaser" style="padding: 5px; font-size: 120%;"><?php if (isset($_POST['teaser'])) { echo $_POST['teaser']; } ?></textarea><br />
<br />

<strong>Topstory image:</strong><br />
	<select name="topstory" id="topstory" style="width:300px" style="padding: 5px; font-size: 120%;">
	<option value="./images/block_32.png"></option>
	<?php
		$newsimages = opendir(CWD . '/images/news/') or die('Error');  
		while($images = @readdir($newsimages)) 
		{  
			$ext = pathinfo($images, PATHINFO_EXTENSION);
			if(!is_dir($images) && ($ext == "png" || $ext == "gif"))
			{
				if(preg_match("/".$images."/", $data['image']))
				$selected = 'selected="selected"';
				else
				$selected = '';
  				echo '<option value="../images/news/'.$images.'" '.$selected.'>'.$images.'</option>';  
				}
		}
		closedir($newsimages);  
	?>
	</select>
	
</div>

<div style="margin-left: 20px; padding: 10px; float: left; text-align: center; vertical-align: middle;">
	<img src="<?php echo "../" . $data['image']; ?>" id="ImagePreview" />
</div>

<div style="clear: both;"></div>

<br /><br />

<textarea class="wysiwyg" id="content" name="content" style="width:80%"><?php if (isset($_POST['content'])) { echo $_POST['content']; } ?></textarea>
<br />
<br />

<input type="submit" value="Update article">&nbsp;
<input type="button" value="Cancel" onclick="window.location = 'index.php?_cmd=news';">

</form>


<?php

require_once "bottom.php";

?>