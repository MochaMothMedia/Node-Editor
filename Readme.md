# Developer Console

A lightweight and extendible developer console system for Unity.

## Installation
Follow the steps [Here](https://github.com/FedoraDevStudios/Installation-Unity) to add this package to your Unity project using this package's URL.

## Usage
### Add to Scene
In the package, there is a prefab in the example provided that comes with everything set up. Add this prefab to a canvas and modify it to suit your needs. I would recommend creating a prefab variant from the provided one for ease of use later. You will also need to add a script to hide and show the console window as this is not preconfigured.

### Create a Command
If you're using `asmdef` files, you should only need to reference `_FedoraDev.DeveloperConsole`. You can reference `_FedoraDev.DeveloperConsole.Implementations` as well, however this will create hard dependencies and isn't the intended way to use this system.

For a quick example on how to create your own command, you can take a look at some default implementations provided. These commands are not complex and are easy to follow. You can create a copy of one of these commands to start. Let's review one here.

```c#
public class EchoCommand : IConsoleCommand
{
	public string Name => "echo";
	public string Usage => "echo {text to display}";
	public IDeveloperConsole DeveloperConsole { get => _developerConsole; set => _developerConsole = value; }

	IDeveloperConsole _developerConsole;

	public void Execute(ICommandArguments arguments)
	{
		DeveloperConsole.PushMessage(arguments.TextEntered.Substring(arguments.CommandName.Length));
	}

	public string[] GetHelp(ICommandArguments arguments)
	{
		return "Prints the text that follows the command to the console.";
	}
}
```

##### Name
```c#
public string Name => "echo";
```
This tells the console how to refer to this command. When a user types 'echo' at the beginning of the command string, the console will refer to this command.

##### Usage
```c#
public string Usage => "echo {text to display}";
```
This string displays when the command is listed by the console's help command. This is an easy way to tell the user how your command expects its format.

##### DeveloperConsole
```c#
public IDeveloperConsole DeveloperConsole { get => _developerConsole; set => _developerConsole = value; }
IDeveloperConsole _developerConsole;
```
We hold a reference to the Developer Console instance. This property gets assigned when the command is loaded into the console.

##### Execute
```c#
public void Execute(ICommandArguments arguments)
{
	DeveloperConsole.PushMessage(arguments.TextEntered.Substring(arguments.CommandName.Length));
}
```
Here is the meat and potatoes of the command. `ICommandArguments` comes with a lot of useful parts for your command so you don't have to parse the user input manually with each command. More on the interface below.

##### GetHelp
```c#
public string[] GetHelp(ICommandArguments arguments)
{
	return new string[] { "Prints the text that follows the command to the console." };
}
```
When a user types `help echo`, this gives the console the proper strings to print out. This returns a list of strings so that formatting is consistent within the console. In the default implementation, the console indents some parts of the command output for readability.

### Add a Command to the Console
Once you have a command created that implements `IConsoleCommand`, you can add the command to the list on the `ConsoleCommandBehaviour` component located on the Developer Console Game Object. Next time you run the game, your command will be loaded in with the rest. To test, you can use the builtin `help` command to get a list of the available commands.

### Create a Pre Processor
Pre processors allow you to modify command inputs before they're executed which provides you with more control over the entire console. I've created `ClipPreProcessor` which talks to the `Clip` command and allows you to insert copied values into commands dynamically. One good example would be to grab the ID of an entity with one command and then using that same ID as an argument in another.

```C#
public class ClipPreProcessor : IPreProcessCommand
{
	public IDeveloperConsole DeveloperConsole { get; set; }

	public string PreProcess(string input)
	{
		ClipCommand spillCommand = DeveloperConsole.GetCommand<ClipCommand>();
		Regex regex = new Regex(@"\[[0-9]+\]", RegexOptions.ExplicitCapture);
		MatchCollection matches = regex.Matches(input);

		foreach (Match match in matches)
		{
			GroupCollection groups = match.Groups;

			foreach (Group group in groups)
			{
				string intString = group.Value.Substring(1, group.Value.Length - 2);
				if (int.TryParse(intString, out int intValue))
					input = input.Replace(group.Value, spillCommand.GetBuffer(intValue));
			}
		}

		return input;
	}
}
```

##### DeveloperConsole
```C#
public IDeveloperConsole DeveloperConsole { get; set; }
```
This stores the `IDeveloperConsole` that's used for this pre processor. This will be set when the pre processor is registered to the developer console.

##### PreProcess
```C#
public string PreProcess(string input)
{
	ClipCommand spillCommand = DeveloperConsole.GetCommand<ClipCommand>();
	Regex regex = new Regex(@"\[[0-9]+\]", RegexOptions.ExplicitCapture);
	MatchCollection matches = regex.Matches(input);

	foreach (Match match in matches)
	{
		GroupCollection groups = match.Groups;

		foreach (Group group in groups)
		{
			string intString = group.Value.Substring(1, group.Value.Length - 2);
			if (int.TryParse(intString, out int intValue))
				input = input.Replace(group.Value, spillCommand.GetBuffer(intValue));
		}
	}

	return input;
}
```
First, we grab a reference to the `ClipCommand` registered to the console. We use a Regex to find any instances of `[index]` within the command and swap them out for that index from the clip buffer. Afterwords, we return the input and the console will run the new command.

### Add Pre Processor to the Console
Similar to how we add commands to the console, we have a `PreProcessCommandBehaviour` with a reference to the console behaviour. During the awake method, these pre processors are registered to the console.

## Details
### Using Command Arguments
The Command Arguments object contains a few useful nuggets of information for your command. For the below examples, assume the input received at the prompt is as follows:

##### Example
`inventory insert 13324678 -s=5 player -v`

##### TextEntered
```c#
string TextEntered { get; }
```
This is the raw text that the user entered. In most cases, you won't need this information.

##### CommandName
```c#
string CommandName { get; }
```
This is the command name received. In the example, this would be `inventory`.

##### ArgumentQuantity
```c#
int ArgumentQuantity { get; }
```
This returns the quantity of arguments. in the above example, this would be `3`.

##### GetArgument
```c#
string GetArgument(int index);
```
You can retreive your arguments by index. These will be ordered as they appear in the raw command. The following table represents what the example would give you:
```c#
GetArgument(0) => "insert";
GetArgument(1) => "13324678";
GetArgument(2) => "player";
```

##### GetFlag
```c#
string GetFlag(char flagName);
```
Flags differ from arguments as in most cases simply checking if the flag is present is all you need. This does support specific assignment of flags, as well. Given the above example, the following table describes what you have access to:
```c#
GetFlag('s') => "5";
GetFlag('v') => "true"; //This is a string, not a boolean
```

##### Spill Command
By default there is a command called `spill`. If you feed it a command, it will show you everything the arguments object will provide. For example:
`spill inventory insert 13324678 -s=5 player -v`
```
Text Entered: inventory insert 13324678 -s=5 player -v
Command: inventory
Arguments: 3
    0: insert
    1: 13324678
    2: player
Flags: 2
    s: 5
    v: true
```

## Further Reading
### IDeveloperConsole
Every piece of the system is built for expansion. If the current console is not providing what you need, but you already have a ton of commands built out, you can simply create a new implementation of `IDeveloperConsole` and swap it in place without needing to touch the rest of your codebase. Currently, there are 2 implementations of `IDeveloperConsole` included; `DefaultDeveloperConsole` and `DeveloperConsoleBehaviour`. The behaviour handles all things Unity and simply stores a reference to an IDeveloperConsole. By default, it's assigned `DefaultDeveloperConsole`. This would be the easiest place to plug in your own implementation; simply select your `IDeveloperConsole` from the drop down on the component and everything should work seemlessly.

### ConsoleCommandBehaviour
This component is incredibly simple, it just holds a list of `IConsoleCommand` and registers them to the assigned `IDeveloperConsole` on `Awake`.