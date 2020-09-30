using System.Collections;

public class StateCommand {
    private int pos;
    private string code, name;



    public StateCommand(int pos, string code, string name)
    {
        this.pos = pos;
        this.code = code;
        this.name = name;
    }
}
