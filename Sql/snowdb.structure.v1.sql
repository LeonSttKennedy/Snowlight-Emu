-- phpMyAdmin SQL Dump
-- version 3.3.9.2
-- http://www.phpmyadmin.net
--
-- Servidor: localhost
-- Tempo de Geração: Jul 11, 2021 as 04:17 PM
-- Versão do Servidor: 5.5.10
-- Versão do PHP: 5.3.6

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Banco de Dados: `snowdb`
--

-- --------------------------------------------------------

--
-- Estrutura da tabela `achievements`
--

CREATE TABLE IF NOT EXISTS `achievements` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `group_name` varchar(64) NOT NULL DEFAULT 'ACH_',
  `category` varchar(255) NOT NULL DEFAULT 'identity',
  `level` int(11) NOT NULL DEFAULT '1',
  `reward_pixels` int(11) NOT NULL DEFAULT '100',
  `reward_points` int(11) DEFAULT '10',
  `progress_needed` int(11) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=117 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `achievements_to_unlock`
--

CREATE TABLE IF NOT EXISTS `achievements_to_unlock` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL,
  `group_id` varchar(255) NOT NULL DEFAULT '',
  `progress` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `avatar_effects`
--

CREATE TABLE IF NOT EXISTS `avatar_effects` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL,
  `sprite_id` int(11) NOT NULL DEFAULT '0',
  `duration` double NOT NULL DEFAULT '3600',
  `activated` enum('0','1') NOT NULL DEFAULT '0',
  `timestamp_activated` double NOT NULL DEFAULT '0',
  `quantity` int(11) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `badges`
--

CREATE TABLE IF NOT EXISTS `badges` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL,
  `badge_id` int(10) unsigned NOT NULL DEFAULT '0',
  `source_type` enum('achievement','static') NOT NULL DEFAULT 'static',
  `source_data` varchar(64) NOT NULL,
  `slot_id` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=33 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `badge_definitions`
--

CREATE TABLE IF NOT EXISTS `badge_definitions` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `code` varchar(64) NOT NULL,
  `rights_sets` varchar(128) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`,`code`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=127 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `bans`
--

CREATE TABLE IF NOT EXISTS `bans` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL DEFAULT '0',
  `remote_address` varchar(64) NOT NULL DEFAULT '',
  `timestamp_created` double NOT NULL DEFAULT '0',
  `timestamp_expire` double NOT NULL DEFAULT '0',
  `moderator_id` int(10) unsigned NOT NULL DEFAULT '0',
  `reason_text` varchar(255) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `bots`
--

CREATE TABLE IF NOT EXISTS `bots` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `ai_type` varchar(64) NOT NULL,
  `name` varchar(64) NOT NULL,
  `look` varchar(255) NOT NULL,
  `motto` varchar(255) NOT NULL,
  `room_id` int(10) unsigned NOT NULL,
  `pos_start` varchar(16) NOT NULL DEFAULT '0|0|0',
  `pos_serve` varchar(16) NOT NULL DEFAULT '0|0|0',
  `pos_defined_range` text NOT NULL,
  `walk_mode` enum('stand','defined','freeroam') NOT NULL DEFAULT 'stand',
  `enabled` enum('0','1') NOT NULL DEFAULT '1',
  `kickable` enum('0','1') NOT NULL DEFAULT '1',
  `rotation` int(11) NOT NULL DEFAULT '-1',
  `effect` int(11) NOT NULL DEFAULT '0',
  `response_distance` int(11) NOT NULL DEFAULT '5',
  `pet_type_handler_id` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=29 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `bots_speech`
--

CREATE TABLE IF NOT EXISTS `bots_speech` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `bot_id` int(10) unsigned NOT NULL,
  `message` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `bot_responses`
--

CREATE TABLE IF NOT EXISTS `bot_responses` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `bot_id` int(10) unsigned NOT NULL DEFAULT '0',
  `triggers` text NOT NULL,
  `responses` text NOT NULL,
  `response_serve_id` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `catalog`
--

CREATE TABLE IF NOT EXISTS `catalog` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `parent_id` int(10) NOT NULL DEFAULT '0',
  `order_num` int(11) NOT NULL DEFAULT '1',
  `enabled` enum('0','1') NOT NULL DEFAULT '1',
  `title` varchar(64) NOT NULL DEFAULT '',
  `icon` int(11) NOT NULL DEFAULT '0',
  `color` int(11) NOT NULL DEFAULT '0',
  `required_right` varchar(64) NOT NULL DEFAULT '',
  `visible` enum('0','1') NOT NULL DEFAULT '1',
  `dummy_page` enum('0','1') NOT NULL DEFAULT '0',
  `coming_soon` enum('0','1') NOT NULL DEFAULT '0',
  `template` varchar(32) NOT NULL DEFAULT 'default_3x3',
  `page_strings_1` text COMMENT 'Delimiter: |',
  `page_strings_2` text COMMENT 'Delimiter: |',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=152 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `catalog_items`
--

CREATE TABLE IF NOT EXISTS `catalog_items` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `page_id` int(11) NOT NULL,
  `base_id` int(10) unsigned NOT NULL,
  `preset_flags` varchar(32) NOT NULL DEFAULT '',
  `name` varchar(64) NOT NULL,
  `cost_credits` int(11) NOT NULL,
  `cost_pixels` int(11) NOT NULL,
  `enabled` enum('0','1') NOT NULL DEFAULT '1',
  `amount` int(11) NOT NULL DEFAULT '1',
  `club_restriction` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=50283 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `catalog_marketplace_data`
--

CREATE TABLE IF NOT EXISTS `catalog_marketplace_data` (
  `id` int(12) NOT NULL AUTO_INCREMENT,
  `sprite_id` int(7) unsigned NOT NULL,
  `sold` int(7) NOT NULL DEFAULT '0',
  `daily_sold` int(7) NOT NULL DEFAULT '0',
  `avgprice` int(9) NOT NULL DEFAULT '0',
  `daysago` int(5) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `catalog_marketplace_offers`
--

CREATE TABLE IF NOT EXISTS `catalog_marketplace_offers` (
  `offer_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `item_id` int(10) unsigned NOT NULL,
  `user_id` int(10) unsigned NOT NULL,
  `asking_price` int(11) NOT NULL,
  `total_price` int(11) NOT NULL DEFAULT '0',
  `public_name` text NOT NULL,
  `sprite_id` int(11) NOT NULL,
  `item_type` enum('1','2') NOT NULL DEFAULT '1',
  `timestamp` double NOT NULL,
  `state` enum('1','2') NOT NULL DEFAULT '1',
  `extra_data` text NOT NULL,
  `limited_number` int(11) NOT NULL DEFAULT '0',
  `limited_stack` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`offer_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=134 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `catalog_subscriptions`
--

CREATE TABLE IF NOT EXISTS `catalog_subscriptions` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(64) NOT NULL DEFAULT 'HABBO_CLUB_BASIC_1_MONTH',
  `type` enum('vip','upgrade','basic') NOT NULL DEFAULT 'basic',
  `cost_credits` int(10) NOT NULL DEFAULT '20',
  `length_days` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=7 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `catalog_subscriptions_gifts`
--

CREATE TABLE IF NOT EXISTS `catalog_subscriptions_gifts` (
  `item_id` int(10) NOT NULL,
  `item_name` varchar(100) NOT NULL,
  `sprite_id` int(10) NOT NULL,
  `days_need` int(10) NOT NULL DEFAULT '1',
  `isvip` enum('0','1') NOT NULL DEFAULT '0',
  PRIMARY KEY (`item_id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estrutura da tabela `characters`
--

CREATE TABLE IF NOT EXISTS `characters` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `account_uid` int(10) unsigned NOT NULL DEFAULT '0',
  `username` varchar(15) NOT NULL DEFAULT '',
  `real_name` varchar(64) NOT NULL DEFAULT '',
  `motto` varchar(255) NOT NULL DEFAULT 'uberHotel Newbie',
  `figure` varchar(255) NOT NULL DEFAULT 'hr-165-34.sh-290-92.ch-215-84.hd-180-1.lg-280-64',
  `gender` enum('M','F') NOT NULL DEFAULT 'M',
  `credits_balance` int(10) NOT NULL DEFAULT '100',
  `activity_points_balance` int(10) NOT NULL DEFAULT '2000',
  `activity_points_last_update` double NOT NULL DEFAULT '0',
  `score` int(11) NOT NULL DEFAULT '0',
  `privacy_accept_friends` enum('0','1') NOT NULL DEFAULT '1',
  `home_room` int(10) unsigned NOT NULL DEFAULT '0',
  `auth_ticket` varchar(64) NOT NULL DEFAULT '',
  `last_ip` varchar(64) NOT NULL DEFAULT '',
  `config_volume` int(11) NOT NULL DEFAULT '100',
  `timestamp_lastvisit` double(11,0) NOT NULL DEFAULT '0',
  `timestamp_created` double NOT NULL DEFAULT '0',
  `moderation_tickets` int(11) NOT NULL DEFAULT '0',
  `moderation_tickets_abusive` int(11) NOT NULL DEFAULT '0',
  `moderation_tickets_cooldown` double NOT NULL DEFAULT '0',
  `moderation_bans` int(11) NOT NULL DEFAULT '0',
  `moderation_cautions` int(11) NOT NULL DEFAULT '0',
  `moderation_muted_until_timestamp` double NOT NULL,
  `respect_points` int(11) NOT NULL DEFAULT '0',
  `respect_credit_humans` int(11) NOT NULL DEFAULT '3',
  `respect_credit_pets` int(11) NOT NULL DEFAULT '3',
  `marketplace_tickets` int(11) NOT NULL DEFAULT '0',
  `last_respect_update` double NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=4 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `drink_sets`
--

CREATE TABLE IF NOT EXISTS `drink_sets` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `drinks` varchar(255) COLLATE latin1_german1_ci NOT NULL,
  `internal_comment` text COLLATE latin1_german1_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci AUTO_INCREMENT=8 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `favorites`
--

CREATE TABLE IF NOT EXISTS `favorites` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL,
  `room_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `flat_categories`
--

CREATE TABLE IF NOT EXISTS `flat_categories` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `title` varchar(64) NOT NULL DEFAULT '',
  `order_num` int(11) NOT NULL DEFAULT '0',
  `visible` enum('0','1') NOT NULL DEFAULT '1',
  `enabled` enum('0','1') NOT NULL DEFAULT '1',
  `allow_trading` enum('0','1') NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=11 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `help_categories`
--

CREATE TABLE IF NOT EXISTS `help_categories` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(128) NOT NULL,
  `visible` enum('1','0') NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=7 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `help_topics`
--

CREATE TABLE IF NOT EXISTS `help_topics` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `category` int(10) unsigned NOT NULL,
  `title` varchar(128) NOT NULL,
  `body` text NOT NULL,
  `priority` enum('1','2','0') NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=12 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `ignores`
--

CREATE TABLE IF NOT EXISTS `ignores` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL,
  `ignore_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `interstitials`
--

CREATE TABLE IF NOT EXISTS `interstitials` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `url` varchar(255) NOT NULL,
  `image` varchar(255) NOT NULL,
  `views` int(11) NOT NULL,
  `enabled` enum('1','0') NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=6 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `items`
--

CREATE TABLE IF NOT EXISTS `items` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `definition_id` int(10) unsigned NOT NULL DEFAULT '0',
  `user_id` int(10) unsigned NOT NULL DEFAULT '0',
  `room_id` int(10) unsigned NOT NULL DEFAULT '0',
  `room_pos` varchar(16) NOT NULL DEFAULT '0|0|0',
  `room_wall_pos` varchar(64) NOT NULL DEFAULT '',
  `room_rot` int(11) NOT NULL DEFAULT '0',
  `flags` text NOT NULL,
  `flags_display` text NOT NULL,
  `untradable` enum('0','1') NOT NULL DEFAULT '0',
  `expire_timestamp` double NOT NULL DEFAULT '0',
  `soundmanager_id` int(10) unsigned NOT NULL DEFAULT '0',
  `soundmanager_order` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`),
  KEY `room_id` (`room_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=58 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `item_definitions`
--

CREATE TABLE IF NOT EXISTS `item_definitions` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `sprite_id` int(10) unsigned NOT NULL,
  `name` varchar(128) NOT NULL DEFAULT 'new_item',
  `type` varchar(32) NOT NULL DEFAULT 's',
  `behavior` varchar(32) NOT NULL DEFAULT 'switch',
  `behavior_data` int(11) NOT NULL DEFAULT '2',
  `stacking_behavior` enum('ignore','terminator','initiator','disable','normal') NOT NULL DEFAULT 'normal',
  `size_x` int(11) NOT NULL DEFAULT '1',
  `size_y` int(11) NOT NULL DEFAULT '1',
  `height` float NOT NULL DEFAULT '1',
  `allow_recycling` enum('0','1') NOT NULL DEFAULT '1',
  `allow_trading` enum('0','1') NOT NULL DEFAULT '1',
  `allow_selling` enum('0','1') NOT NULL DEFAULT '1',
  `allow_gifting` enum('0','1') NOT NULL DEFAULT '1',
  `allow_inventory_stacking` enum('0','1') NOT NULL DEFAULT '1',
  `walkable` enum('1','2','0') NOT NULL DEFAULT '0',
  `room_limit` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=2911 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `messenger_friendships`
--

CREATE TABLE IF NOT EXISTS `messenger_friendships` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_1_id` int(11) unsigned NOT NULL,
  `user_2_id` int(11) unsigned NOT NULL,
  `confirmed` int(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `user_1_id` (`user_1_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=5 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `moderation_action_log`
--

CREATE TABLE IF NOT EXISTS `moderation_action_log` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `moderator_id` int(10) unsigned NOT NULL,
  `moderator_name` varchar(16) NOT NULL,
  `action_descr` varchar(255) NOT NULL,
  `action_detail` text NOT NULL,
  `timestamp` double NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `moderation_chatlogs`
--

CREATE TABLE IF NOT EXISTS `moderation_chatlogs` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `timestamp` double NOT NULL DEFAULT '0',
  `user_id` int(10) unsigned NOT NULL DEFAULT '0',
  `room_id` int(10) unsigned NOT NULL DEFAULT '0',
  `message` varchar(255) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=8 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `moderation_presets`
--

CREATE TABLE IF NOT EXISTS `moderation_presets` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `type` enum('user','room') NOT NULL DEFAULT 'user',
  `message` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=2 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `moderation_preset_action_categories`
--

CREATE TABLE IF NOT EXISTS `moderation_preset_action_categories` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `caption` varchar(32) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=12 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `moderation_preset_action_messages`
--

CREATE TABLE IF NOT EXISTS `moderation_preset_action_messages` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `parent_id` int(10) unsigned NOT NULL,
  `caption` varchar(32) NOT NULL,
  `message_text` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=15 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `moderation_tickets`
--

CREATE TABLE IF NOT EXISTS `moderation_tickets` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `category` int(10) unsigned NOT NULL DEFAULT '0',
  `status` enum('5','4','3','2','1','0') NOT NULL DEFAULT '0',
  `reported_user_id` int(10) unsigned NOT NULL DEFAULT '0',
  `reportee_user_id` int(10) unsigned NOT NULL DEFAULT '0',
  `moderator_user_id` int(10) unsigned NOT NULL DEFAULT '0',
  `room_id` int(10) unsigned NOT NULL DEFAULT '0',
  `timestamp` double NOT NULL DEFAULT '0',
  `message` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `navigator_event_search_categories`
--

CREATE TABLE IF NOT EXISTS `navigator_event_search_categories` (
  `query` varchar(48) NOT NULL,
  `category_id` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`query`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Estrutura da tabela `navigator_frontpage`
--

CREATE TABLE IF NOT EXISTS `navigator_frontpage` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `parent_id` int(10) unsigned NOT NULL DEFAULT '0',
  `room_id` int(10) unsigned NOT NULL DEFAULT '0',
  `is_category` enum('1','0') NOT NULL DEFAULT '0',
  `category_autoexpand` enum('0','1') NOT NULL DEFAULT '0',
  `display_type` enum('details','banner') NOT NULL DEFAULT 'details',
  `name` varchar(64) NOT NULL DEFAULT '',
  `descr` varchar(255) NOT NULL DEFAULT '',
  `image_type` enum('external','internal') NOT NULL DEFAULT 'internal',
  `image_src` varchar(128) NOT NULL DEFAULT '',
  `banner_label` varchar(64) NOT NULL DEFAULT '',
  `order_num` int(11) NOT NULL DEFAULT '0',
  `enabled` enum('0','1') NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=17 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `new_items`
--

CREATE TABLE IF NOT EXISTS `new_items` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL DEFAULT '0',
  `tab_id` int(11) NOT NULL DEFAULT '1',
  `item_id` int(11) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `pets`
--

CREATE TABLE IF NOT EXISTS `pets` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(16) NOT NULL,
  `type` int(11) NOT NULL DEFAULT '0',
  `race` int(11) NOT NULL DEFAULT '0',
  `color` varchar(11) DEFAULT NULL,
  `user_id` int(11) unsigned NOT NULL DEFAULT '0',
  `room_id` int(11) unsigned NOT NULL DEFAULT '0',
  `room_pos` varchar(16) NOT NULL DEFAULT '0|0|0',
  `timestamp` double NOT NULL DEFAULT '0',
  `experience` int(11) NOT NULL DEFAULT '0',
  `energy` int(11) NOT NULL DEFAULT '120',
  `happiness` int(11) NOT NULL DEFAULT '100',
  `score` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`),
  KEY `room_id` (`room_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `pet_races`
--

CREATE TABLE IF NOT EXISTS `pet_races` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `pet_type` int(11) NOT NULL DEFAULT '0',
  `data1` int(11) NOT NULL DEFAULT '0',
  `data2` int(11) NOT NULL DEFAULT '0',
  `data3` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `pet_type` (`pet_type`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=160 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `pet_tricks`
--

CREATE TABLE IF NOT EXISTS `pet_tricks` (
  `type` int(3) NOT NULL,
  `trick` varchar(32) NOT NULL,
  KEY `type` (`type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Estrutura da tabela `quests`
--

CREATE TABLE IF NOT EXISTS `quests` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `category` varchar(32) NOT NULL DEFAULT '',
  `series_number` int(11) NOT NULL DEFAULT '0',
  `goal_type` int(10) NOT NULL DEFAULT '0',
  `goal_data` int(10) unsigned NOT NULL DEFAULT '0',
  `name` varchar(32) NOT NULL DEFAULT '',
  `reward` int(11) NOT NULL DEFAULT '10',
  `data_bit` varchar(2) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=34 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `recycler_rewards`
--

CREATE TABLE IF NOT EXISTS `recycler_rewards` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `chance_level` int(11) NOT NULL,
  `item_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=29 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `rights`
--

CREATE TABLE IF NOT EXISTS `rights` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `set_id` int(10) unsigned NOT NULL,
  `right_id` varchar(128) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=14 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `rooms`
--

CREATE TABLE IF NOT EXISTS `rooms` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `type` enum('flat','public') NOT NULL DEFAULT 'flat',
  `owner_id` int(11) unsigned NOT NULL,
  `name` varchar(64) NOT NULL DEFAULT '',
  `description` varchar(128) NOT NULL DEFAULT '',
  `pub_internal_name` varchar(64) NOT NULL DEFAULT '',
  `tags` varchar(65) NOT NULL DEFAULT '',
  `access_type` enum('password','doorbell','open') NOT NULL DEFAULT 'open',
  `password` varchar(64) NOT NULL DEFAULT '',
  `category` int(10) NOT NULL DEFAULT '0',
  `current_users` int(11) NOT NULL DEFAULT '0',
  `max_users` int(11) NOT NULL DEFAULT '25',
  `swfs` varchar(64) NOT NULL DEFAULT '',
  `score` int(11) NOT NULL DEFAULT '0',
  `icon` varchar(64) NOT NULL DEFAULT '1|0|1|4,1',
  `model` varchar(64) NOT NULL DEFAULT 'model_a',
  `allow_pets` enum('0','1') NOT NULL DEFAULT '1',
  `allow_pet_eating` enum('0','1') NOT NULL DEFAULT '0',
  `disable_blocking` enum('1','0') NOT NULL DEFAULT '1',
  `hide_walls` enum('0','1') NOT NULL DEFAULT '0',
  `thickness_wall` int(11) NOT NULL DEFAULT '0',
  `thickness_floor` int(11) NOT NULL DEFAULT '0',
  `decorations` varchar(128) NOT NULL DEFAULT 'landscape=0.0',
  `badge_id` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=19 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `room_models`
--

CREATE TABLE IF NOT EXISTS `room_models` (
  `id` varchar(64) NOT NULL,
  `type` enum('public','flat') NOT NULL DEFAULT 'flat',
  `heightmap` text NOT NULL,
  `enabled` enum('0','1') NOT NULL DEFAULT '1',
  `door_x` int(11) NOT NULL,
  `door_y` int(11) NOT NULL,
  `door_z` double NOT NULL,
  `door_dir` int(11) NOT NULL,
  `subscription_requirement` enum('2','1','0') NOT NULL,
  `max_users` int(11) NOT NULL DEFAULT '30',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Estrutura da tabela `room_rights`
--

CREATE TABLE IF NOT EXISTS `room_rights` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `room_id` int(10) unsigned NOT NULL,
  `user_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`id`),
  KEY `room_id` (`room_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `room_triggers`
--

CREATE TABLE IF NOT EXISTS `room_triggers` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `room_id` int(10) unsigned NOT NULL,
  `room_pos` varchar(16) CHARACTER SET utf8 NOT NULL DEFAULT '0|0|0',
  `action` enum('roller','teleport') CHARACTER SET utf8 NOT NULL DEFAULT 'roller',
  `to_room_id` int(10) unsigned NOT NULL,
  `to_room_pos` varchar(16) CHARACTER SET utf8 NOT NULL DEFAULT '0|0|0',
  `to_room_dir` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=37 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `room_visits`
--

CREATE TABLE IF NOT EXISTS `room_visits` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL DEFAULT '0',
  `room_id` int(10) unsigned NOT NULL DEFAULT '0',
  `timestamp_entered` double NOT NULL DEFAULT '0',
  `timestamp_left` double NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=379 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `server_ingame_texts`
--

CREATE TABLE IF NOT EXISTS `server_ingame_texts` (
  `identifier` varchar(50) NOT NULL,
  `display_text` text NOT NULL,
  PRIMARY KEY (`identifier`),
  KEY `identifier` (`identifier`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estrutura da tabela `server_settings`
--

CREATE TABLE IF NOT EXISTS `server_settings` (
  `activitypoints_enabled` enum('0','1') CHARACTER SET utf8 NOT NULL DEFAULT '1',
  `more_activitypoints_for_vip_users` enum('0','1') CHARACTER SET utf8 NOT NULL DEFAULT '1',
  `activitypoints_interval` int(11) NOT NULL DEFAULT '1800' COMMENT '1800 = 30 min',
  `activitypoints_credits_amount` int(11) NOT NULL DEFAULT '0',
  `more_activitypoints_credits_amount` int(11) NOT NULL DEFAULT '25',
  `activitypoints_pixels_amount` int(11) NOT NULL DEFAULT '50',
  `more_activitypoints_pixels_amount` int(11) NOT NULL DEFAULT '50',
  `motd_enabled` enum('0','1') CHARACTER SET utf8 NOT NULL DEFAULT '1',
  `motd_type` enum('NotificationMessageComposer','MessageOfTheDayComposer') CHARACTER SET utf8 NOT NULL DEFAULT 'MessageOfTheDayComposer',
  `motd_text` text CHARACTER SET utf8 NOT NULL COMMENT 'Example: Text|Link',
  `login_badge_enabled` enum('0','1') CHARACTER SET utf8 NOT NULL DEFAULT '0',
  `login_badge_id` int(10) unsigned NOT NULL DEFAULT '33',
  `moderation_actionlogs_enabled` enum('0','1') CHARACTER SET utf8 NOT NULL DEFAULT '0',
  `moderation_chatlogs_enabled` enum('0','1') CHARACTER SET utf8 NOT NULL DEFAULT '0',
  `moderation_roomlogs_enabled` enum('0','1') CHARACTER SET utf8 NOT NULL DEFAULT '0',
  `marketplace_enabled` enum('0','1') CHARACTER SET utf8 NOT NULL DEFAULT '1',
  `marketplace_tax` int(2) NOT NULL DEFAULT '1',
  `marketplace_tokens_price` int(2) NOT NULL DEFAULT '1',
  `marketplace_premium_tokens` int(2) NOT NULL DEFAULT '10',
  `marketplace_default_tokens` int(2) NOT NULL DEFAULT '5',
  `marketplace_min_price` int(2) NOT NULL DEFAULT '1',
  `marketplace_max_price` int(9) NOT NULL DEFAULT '10000',
  `marketplace_offer_hours` int(2) NOT NULL DEFAULT '48',
  `marketplace_avarage_days` int(2) NOT NULL DEFAULT '7',
  `max_favorites_per_user` int(2) NOT NULL DEFAULT '30',
  `max_furni_per_room` int(6) NOT NULL DEFAULT '500',
  `max_furni_stacking` int(2) NOT NULL DEFAULT '12',
  `max_pets_per_room` int(2) NOT NULL DEFAULT '10',
  `max_rooms_per_user` int(2) NOT NULL DEFAULT '15',
  `wordfilter_maximum_count` int(2) NOT NULL DEFAULT '5',
  `wordfilter_time_muted` int(11) NOT NULL DEFAULT '300' COMMENT '300 = 5 min'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estrutura da tabela `server_statistics`
--

CREATE TABLE IF NOT EXISTS `server_statistics` (
  `server_status` enum('0','1','2') NOT NULL DEFAULT '0',
  `server_ver` text NOT NULL,
  `active_connections` int(11) NOT NULL DEFAULT '0',
  `all_time_player_peak` int(11) NOT NULL DEFAULT '0',
  `daily_player_peak` int(11) NOT NULL DEFAULT '0',
  `rooms_loaded` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Estrutura da tabela `songs`
--

CREATE TABLE IF NOT EXISTS `songs` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(64) NOT NULL,
  `artist` varchar(32) NOT NULL,
  `song_data` text NOT NULL,
  `length` double NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=17 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `static_objects`
--

CREATE TABLE IF NOT EXISTS `static_objects` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `room_id` int(10) unsigned NOT NULL DEFAULT '0',
  `name` varchar(64) NOT NULL DEFAULT '',
  `position` varchar(16) NOT NULL DEFAULT '0|0|0',
  `size_x` int(11) NOT NULL DEFAULT '1',
  `size_y` int(11) NOT NULL DEFAULT '1',
  `rotation` int(11) NOT NULL DEFAULT '0',
  `height` float NOT NULL DEFAULT '1',
  `walkable` enum('0','1') DEFAULT '0',
  `is_seat` enum('0','1') DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=998 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `tags`
--

CREATE TABLE IF NOT EXISTS `tags` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `tag` varchar(32) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `user_achievements`
--

CREATE TABLE IF NOT EXISTS `user_achievements` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL,
  `group_id` varchar(255) NOT NULL DEFAULT '',
  `level` int(11) NOT NULL DEFAULT '0',
  `progress` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=16 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `user_gifts`
--

CREATE TABLE IF NOT EXISTS `user_gifts` (
  `item_id` int(10) unsigned NOT NULL,
  `base_id` int(10) unsigned NOT NULL,
  `amount` int(11) NOT NULL,
  `extra_data` text NOT NULL,
  KEY `item_id` (`item_id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estrutura da tabela `user_quests`
--

CREATE TABLE IF NOT EXISTS `user_quests` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL DEFAULT '0',
  `quest_id` int(10) unsigned NOT NULL DEFAULT '0',
  `progress` int(11) NOT NULL DEFAULT '0',
  `is_current` enum('0','1') NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=9 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `user_subscriptions`
--

CREATE TABLE IF NOT EXISTS `user_subscriptions` (
  `user_id` int(10) unsigned NOT NULL,
  `subscription_level` enum('2','1','0') NOT NULL DEFAULT '0',
  `timestamp_created` double(11,0) NOT NULL DEFAULT '0',
  `timestamp_expire` double(11,0) NOT NULL DEFAULT '0',
  `past_time_hc` double NOT NULL DEFAULT '0' COMMENT '0',
  `past_time_vip` double NOT NULL DEFAULT '0' COMMENT '0',
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Estrutura da tabela `vouchers`
--

CREATE TABLE IF NOT EXISTS `vouchers` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `code` varchar(32) NOT NULL DEFAULT '',
  `value_credits` int(11) NOT NULL DEFAULT '0',
  `value_pixels` int(11) DEFAULT '0',
  `value_furni` varchar(64) DEFAULT '',
  `uses` int(11) NOT NULL DEFAULT '1',
  `enabled` enum('0','1') NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `wardrobe`
--

CREATE TABLE IF NOT EXISTS `wardrobe` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(11) unsigned NOT NULL DEFAULT '0',
  `slot_id` int(11) NOT NULL DEFAULT '0',
  `figure` varchar(255) NOT NULL DEFAULT '',
  `gender` enum('F','M') NOT NULL DEFAULT 'M',
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Estrutura da tabela `wordfilter`
--

CREATE TABLE IF NOT EXISTS `wordfilter` (
  `word` varchar(100) NOT NULL,
  PRIMARY KEY (`word`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
