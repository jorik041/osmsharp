--drop tables
if object_id('dbo.node', 'U') is not null
	drop table dbo.node
if object_id('dbo.node_tags', 'U') is not null
	drop table dbo.node_tags 
if object_id('dbo.way', 'U') is not null
	drop table dbo.way 
if object_id('dbo.way_tags', 'U') is not null
	drop table dbo.way_tags 
if object_id('dbo.way_nodes', 'U') is not null
	drop table dbo.way_nodes 
if object_id('dbo.relation', 'U') is not null
	drop table dbo.relation 
if object_id('dbo.relation_tags', 'U') is not null
	drop table dbo.relation_tags 
if object_id('dbo.relation_members', 'U') is not null
	drop table dbo.relation_members 