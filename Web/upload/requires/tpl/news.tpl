<?php
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/


if(!isset($_GET['id']))
{
	if(!isset($_GET['categoryid']))
	{
		$setid = mysql_fetch_assoc(mysql_query("SELECT * FROM articles ORDER BY published DESC LIMIT 1"));
		TemplateManager::WriteLine("<script type='text/javascript'>window.top.location='http://".SITE_DOMAIN."/news.php?id=" . $setid['id'] . "-" . $setid['seo_link'] ."';</script>");
	}
}

$newsid = explode("-", $_GET['id']);
$articleqquerystring = isset($_GET['categoryid']) ? "SELECT * FROM articles WHERE category_id = '" . $_GET['categoryid'] . "' ORDER BY published DESC LIMIT 1" : "SELECT * FROM articles WHERE id = '$newsid[0]' LIMIT 1";
$articleq = mysql_query($articleqquerystring);
$article = mysql_fetch_array($articleq);

$author = mysql_result(mysql_query("SELECT username FROM characters WHERE id = '".$article['author']."' LIMIT 1"), 0);
$category = mysql_result(mysql_query("SELECT caption FROM articles_categories WHERE id = '".$article['category_id']."' LIMIT 1"), 0);
$recentstoriesq = isset($_GET['categoryid']) ? "SELECT * FROM articles WHERE category_id = '" . $_GET['categoryid'] . "' ORDER BY published DESC LIMIT 25" : "SELECT * FROM articles ORDER BY published DESC LIMIT 25";
?>
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
<div class="column1" id="column1">
<h2><?php TemplateManager::WriteLine($article["title"]); ?></h2>
<small><?php TemplateManager::WriteLine(@date("D, d M Y H:i:s", $article['published'])); ?> <a href="<?php TemplateManager::WriteLine('http://' . SITE_DOMAIN . '/news.php?categoryid=' .$article['category_id']); ?>"><?php TemplateManager::WriteLine($category); ?></a></small>
<p><em><?php TemplateManager::WriteLine($article['shortstory']); ?></em></p>
<br />
<?php TemplateManager::WriteLine($article["longstory"]); ?>
<br />
<p><h3><?php TemplateManager::WriteLine($author); ?></strong></h3>
<br /><br />
</div>

<div class="column2" id="column2">
<div class="charentry">
<div class="inner">
<div class="left" style="width: 65%;">
<h3>Recent news articles</h3><br />
<?php
$num = mysql_num_rows(mysql_query($recentstoriesq));
if($num > 0)
{
?>
<div id="newsblock">

	<div id="newslist">
		<ul class="newslist-inner">
		<?php
		$query = mysql_query($recentstoriesq);
		while($news = mysql_fetch_assoc($query))
		{
			if($color == 'n')
			$color='alt';
			else
			$color='n';

			if($article['id'] == $news['id'])
			{
				TemplateManager::WriteLine('<li class="'.$color.'">
						<b>'.$news['title'].'</b><br />
						<small>'.@date("D, d M Y H:i:s", $news['published']).'</small><br />
					</li>');
			}
			else
			{
				TemplateManager::WriteLine('<li class="'.$color.'">
						<a href="http://'.SITE_DOMAIN.'/news.php?id=' . $news['id'] . '-' . $news['seo_link'] . '">'.$news['title'].'</a><br />
						<small>'.@date("D, d M Y H:i:s", $news['published']).'</small><br />
					</li>');
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

<div class="clear">
</div>
</div>
