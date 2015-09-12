using UnityEngine;
using System.Collections;

using IronPython;
using IronPython.Modules;

public class PythonBehaviour : MonoBehaviour {

	void Start() {
		var engine = IronPython.Hosting.Python.CreateEngine();
		//
		// string dir = Path.GetDirectoryName(scriptPath);
		// ICollection<string> paths = engine.GetSearchPaths();
		// if (!String.IsNullOrWhitespace(dir)) {
		//     paths.Add(dir);
		// } else {
		//     paths.Add(Environment.CurrentDirectory);
		// }
		// engine.SetSearchPaths(paths);

		// and the scope (ie, the python namespace)
		var scope = engine.CreateScope();
		// execute a string in the interpreter and grab the variable
		string example = "output = 'hello world'";
		var source = engine.CreateScriptSourceFromString(example);
		source.Execute(scope);
		string came_from_script = scope.GetVariable<string>("output");
		// Should be what we put into 'output' in the script.
		Debug.Log(came_from_script);
	}
}
