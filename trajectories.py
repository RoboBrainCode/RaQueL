def trajectory(n):
	'''returns affordances of object n(record)'''
	if type(n[0]).__name__ == 'Node':
		from handle import handle
		return fetch("({handle:" + handle(n[0]) + "})-[:IS_TRAJECTORY_OF]->(v)")