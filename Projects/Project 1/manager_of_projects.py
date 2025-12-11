# Manager of projects
def interface():
    print('Greetings, my Lord! We praise you.')
    while True:
        print('''What do you want us to do?
                 1. Create a new note
                 2. Delete your note
                 3. Search for a note
                 4. View all the notes
                 5. Exit the program
                 6. Delete with keyword''')
        answer = input()
        match answer:
            case '1':
                create()
            case '2':
                delete()
            case '3':
                search()
            case '4':
                view()
            case '5':
                close()
            case '6':
                delete_with_keyword()
            case _:
                print('You should choose a number from the list.')
                continue

def get_notes():
    try:
        with open('notes.txt', 'r', encoding = 'UTF-8') as file:
            notes = file.readlines()
        if not notes:
            print('You have not created any notes yet. \n')
            return None
        return notes
    except FileNotFoundError:
        print('File not found!')
        return None

def create():
    note = input('Type a new note: ')
    with open('notes.txt', 'a', encoding = 'UTF-8') as file:
        file.write(note + '\n')
    print('Your note was successfully created! \n')

def delete():
    notes = view(return_notes=True)
    if notes == None:
        return None

    note_index = input('Which note do you want to delete? — ')
    if note_index.isdigit() == False:
        print('You should enter the NUMBER of the note you want to delete.')
        return None
    note_index = int(note_index)

    if note_index > 0 and len(notes) >= note_index:
        notes_except_deleted = []
        for i in range(len(notes)):
            if note_index != i+1:
                notes_except_deleted.append(notes[i])
        with open('notes.txt', 'w', encoding = 'UTF-8') as file:
            file.writelines(notes_except_deleted)
        print('Your note was successfully deleted! \n')
        return None
    print(f'There is no note with number "{note_index}". \n')

def search():
    notes = get_notes()
    if notes == None:
        return None
    
    notes_found = []
    search_text = input('Type the search text: ')
    for i in range(len(notes)):
        if search_text in notes[i]:
            notes_found.append(notes[i])
    
    lnf = len(notes_found)
    if lnf != 0:
        if lnf == 1:
            word = 'note'
        else:
            word = 'notes'
        print(f'{lnf} {word} found: \n')
        for i, n in enumerate(notes_found, 1):
            print(f'{i}. {n}')
    else:
        print('No notes containing this text were found. \n')
        return None

def view(return_notes=False):
    notes = get_notes()
    if notes == None:
        return None
    print('List of your notes: \n')
    for i, n in enumerate(notes, 1):
        print(f'{i}. {n}')
    if return_notes:
        return notes

def delete_with_keyword():
    notes = get_notes()
    if notes == None:
        return None

    keyword = input('I shall delete notes containing this text: ')
    kill_list = []
    for i in range(len(notes)):
        if keyword in notes[i]:
            kill_list.append(i)
    k = 0
    for i in kill_list:
        notes.remove(notes[i-k])
        k += 1
    with open('notes.txt', 'w', encoding = 'UTF-8') as file:
        file.writelines(notes)
    print('Each note containing this text was deleted! \n')

def close():
    print('Farewell, my Lord!')
    exit()

if __name__ == '__main__':
    interface()