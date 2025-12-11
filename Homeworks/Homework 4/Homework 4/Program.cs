// Животное и наследники
class Animal
{
    private string name;
    private int age;
    protected string Name{get{return name;} set{name = value;}}
    public int Age{get{return age;} set{age = value;}}
    public Animal(string name1, int age1){name = name1; age = age1;}
    public void Sound(){}
}
class Dog: Animal
{
    public Dog(string name1, int age1): base(name1, age1){}

    public void Gav(){}
}
class Cat: Animal
{
    public Cat(string name1, int age1): base(name1, age1){}
    public void Mav(){}
}

// Студент и наследники
class Student
{
    private string name;
    private int age;
    private string group;
    
    protected string Name{get{return name;} set{name = value;}}
    public int Age{get{return age;} set{age = value;}}
    public string Group{get{return group;} set{group = value;}}
    
    public Student(string name1, int age1, string group1)
    {
        name = name1;
        age = age1;
        group = group1;
    }
    
    public void Study()
    {
        System.Console.WriteLine($"Студент по имени {Name}, которому {Age} лет, учится в группе {Group}.");
    }
}

class Master : Student
{
    public Master(string name1, int age1, string group1) : base(name1, age1, group1){}
    
    public void DefendDiploma()
    {
        System.Console.WriteLine($"Магистр {Name} защищает дипломную работу.");
    }
}

class Bachelor : Student
{
    public Bachelor(string name1, int age1, string group1) : base(name1, age1, group1){}
    
    public void TakeExams()
    {
        System.Console.WriteLine($"Бакалавр {Name} сдает экзамены в группе {Group}.");
    }
}