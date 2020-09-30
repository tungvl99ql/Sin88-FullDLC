using System.Collections;
using System.Collections.Generic;

public class State {
    private int id, mode;
    private string code;

    public List<StateCommand> commands = new List<StateCommand>();
    public Dictionary<int, List<StateCommand>> commandsByPosition;

    public State(int id, string code, int mode)
    {
        this.id = id;
        this.code = code;
        this.mode = mode;
    }
}
