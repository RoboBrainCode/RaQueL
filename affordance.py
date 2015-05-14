def affordance(n):
	'''returns affordances of object n'''
	if type(n[0]).__name__ == 'Node':
		from handle import handle
		return fetch("({handle:" + handle(n) + "})-[:HAS_AFFORDANCE]->(v)")