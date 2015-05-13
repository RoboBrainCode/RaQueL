from py2neo import cypher, core, Node, Relationship
def printingRecords(results):
	for record in results:
		for NoR in record:
			type(NoR)
			if type(NoR).__name__ == 'Node':
				print 'node.handle ',
				print NoR.properties['handle'],
			else:		#py2neo.core.Relationship
				print 'Relationship.keywords ',
				print NoR.properties['keywords'],
		print ""