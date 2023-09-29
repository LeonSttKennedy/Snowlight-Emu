<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_sitemanagement'))
{
	exit;
}

if (isset($_POST['content']))
{
	$title = $_POST['title'];
	$teaser = $_POST['teaser'];
	$topstory = str_replace("../", "", $_POST['topstory']);
	$content = $_POST['content'];
	$seoUrl = $_POST['url'];
	$category = intval($_POST['category']);
	
	$uid = intval(mysql_result(mysql_query("SELECT id FROM characters WHERE username = '" . USER_NAME . "' LIMIT 1"), 0));
	
	if (strlen($seoUrl) < 1 || strlen($title) < 1 || strlen($teaser) < 1 || strlen($content) < 1)
	{
		fMessage('error', 'Please fill in all fields.');
	}
	else
	{
		mysql_query("INSERT INTO articles (id, seo_link, title, category_id, shortstory, longstory, published, author, image) VALUES (NULL, '" . $seoUrl . "', '" . $title . "', '" . $category . "', '" . $teaser . "', '" . $content . "', '" . time() . "', '" . $uid . "', '" . $topstory . "')");
		fMessage('ok', 'News article published.');

		header("Location: index.php?_cmd=news");
		exit;
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
	
	function suggestSEO(el)
	{
		var suggested = el;
	
		suggested = suggested.toLowerCase();
		suggested = suggested.replace(/^\s+/, ''); 
		suggested = suggested.replace(/\s+$/, '');
		suggested = suggested.replace(/[^a-z 0-9]+/g, '');
	
		while (suggested.indexOf(' ') > -1)
		{
			suggested = suggested.replace(' ', '-');
		}
	
		document.getElementById('url').value = suggested;
	}
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
	echo '<option value="' . intval($option['id']) . '" ' . (($option['id'] == $_POST['category']) ? 'selected' : '') . '>' . $option['caption'] . '</option>';
}

?>
</select><br />
<br />

<strong>SEO-friendly URL:</strong><br />
<div style="border: 1px dotted; width: 300px; padding: 5px;">
http://<?php echo SITE_DOMAIN; ?>/news.php?id=[id]-<input type="text" id="url" name="url" value="<?php if (isset($_POST['url'])) { echo $_POST['url']; } ?>" maxlength="120">/<br />
</div>
<small>This will be automatically suggested for you when you type a title. Required for us to be friendly to search engines.</small><br />
<br />

<strong>Frontpage teaser text:</strong><br />
<textarea class="wysiwyg" name="teaser" cols="48" rows="5" style="padding: 5px; font-size: 120%;"><?php if (isset($_POST['teaser'])) { echo $_POST['teaser']; } ?></textarea><br />
<br />

<strong>Topstory image:</strong><br />
	<select name="topstory" id="topstory" style="width:300px">
		<option value="./images/block_32.png"></option>
        <?php
        $newsimages = opendir(CWD . '/images/news/') or die('Error');  
		while($images = @readdir($newsimages)) 
		{  
			$ext = pathinfo($images, PATHINFO_EXTENSION);
			if(!is_dir($images) && ($ext == "png" || $ext == "gif"))
  			echo '<option value="../images/news/'.$images.'">'.$images.'</option>';  
		}
		closedir($newsimages);  
		?>
	</select>
	
</div>

<div style="margin-left: 20px; padding: 10px; float: left; text-align: center; vertical-align: middle;">
	<img src="./images/block_32.png" id="ImagePreview" />	
</div>

<div style="clear: both;"></div>

<br /><br />

<textarea class="wysiwyg" id="content" name="content" style="width:80%"><?php if (isset($_POST['content'])) { echo $_POST['content']; } ?></textarea>

<br />
<br />

<input type="submit" value="Submit">

</form>


<?php

require_once "bottom.php";

?>