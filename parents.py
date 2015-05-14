def parents(n):
	'''returns parents of node n
	to call parents(record_containing_node)'''
	if type(n[0]).__name__ == 'Node':
		from handle import handle
		return fetch("(v)-[:HAS_PARAMETERS]->({handle:" + handle(n) + "})")