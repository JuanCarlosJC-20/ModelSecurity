// Variables globales
let currentPage = 1;
const itemsPerPage = 10;
let currentSection = 'dashboard';
let mockData = {
    users: [],
    persons: [],
    roles: [],
    permissions: [],
    forms: [],
    modules: []
};

// Inicializar datos mock
function initializeMockData() {
    // Módulos
    for (let i = 1; i <= 5; i++) {
        mockData.modules.push({
            id: i,
            name: `Módulo ${i}`,
            active: true,
            created: new Date().toISOString().split('T')[0]
        });
    }

    // Formularios
    for (let i = 1; i <= 8; i++) {
        mockData.forms.push({
            id: i,
            name: `Formulario ${i}`,
            code: `FORM${i}`,
            active: i % 3 !== 0,
            created: new Date().toISOString().split('T')[0],
            modules: [Math.ceil(i / 2)]
        });
    }

    // Permisos
    const permissionTypes = ['Crear', 'Leer', 'Actualizar', 'Eliminar'];
    let permId = 1;
    for (const type of permissionTypes) {
        mockData.permissions.push({
            id: permId,
            name: type,
            code: type.substring(0, 1),
            active: true,
            created: new Date().toISOString().split('T')[0]
        });
        permId++;
    }

    // Roles
    const roleTypes = ['Administrador', 'Usuario', 'Supervisor', 'Invitado'];
    for (let i = 0; i < roleTypes.length; i++) {
        mockData.roles.push({
            id: i + 1,
            name: roleTypes[i],
            description: `Rol de ${roleTypes[i]} con permisos específicos`,
            active: i !== 3,
            created: new Date().toISOString().split('T')[0],
            formPermissions: []
        });
    }

    // Generar relaciones de permisos para roles
    for (const role of mockData.roles) {
        for (const form of mockData.forms) {
            if (role.id === 1) { // Administrador tiene todos los permisos
                mockData.permissions.forEach(perm => {
                    role.formPermissions.push({
                        formId: form.id,
                        permissionId: perm.id
                    });
                });
            } else if (role.id === 2) { // Usuario tiene permisos de lectura en todos los formularios
                role.formPermissions.push({
                    formId: form.id,
                    permissionId: 2 // Leer
                });
            } else if (role.id === 3 && form.id <= 4) { // Supervisor tiene permisos en algunos formularios
                mockData.permissions.forEach(perm => {
                    role.formPermissions.push({
                        formId: form.id,
                        permissionId: perm.id
                    });
                });
            }
        }
    }

    // Personas
    const firstNames = ['Juan', 'María', 'Carlos', 'Ana', 'Pedro', 'Laura', 'José', 'Sofía'];
    const lastNames = ['García', 'Martínez', 'López', 'Rodríguez', 'González', 'Pérez', 'Sánchez', 'Fernández'];
    
    for (let i = 0; i < 15; i++) {
        const firstName = firstNames[Math.floor(Math.random() * firstNames.length)];
        const lastName = lastNames[Math.floor(Math.random() * lastNames.length)];
        mockData.persons.push({
            id: i + 1,
            firstName,
            lastName,
            email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}@example.com`
        });
    }

    // Usuarios
    for (let i = 0; i < 10; i++) {
        mockData.users.push({
            id: i + 1,
            username: `user${i + 1}`,
            code: `U${i + 1}`,
            active: i !== 2,
            personId: i + 1,
            created: new Date().toISOString().split('T')[0],
            roles: i === 0 ? [1] : i < 3 ? [2] : i < 6 ? [3] : [4]
        });
    }
}

// Funciones de utilidad
function displayModal(title, message, onConfirm) {
    const modal = document.getElementById('modal');
    const modalTitle = document.getElementById('modal-title');
    const modalMessage = document.getElementById('modal-message');
    const modalConfirm = document.getElementById('modal-confirm');
    const modalCancel = document.getElementById('modal-cancel');
    const closeBtn = document.querySelector('.close');
    
    modalTitle.textContent = title;
    modalMessage.textContent = message;
    
    modal.style.display = 'block';
    
    modalConfirm.onclick = () => {
        onConfirm();
        modal.style.display = 'none';
    };
    
    modalCancel.onclick = () => {
        modal.style.display = 'none';
    };
    
    closeBtn.onclick = () => {
        modal.style.display = 'none';
    };
    
    window.onclick = (event) => {
        if (event.target === modal) {
            modal.style.display = 'none';
        }
    };
}

function showSection(sectionId) {
    document.querySelectorAll('.section').forEach(section => {
        section.classList.remove('active');
    });
    document.getElementById(sectionId).classList.add('active');
    
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });
    document.querySelector(`.nav-link[data-section="${sectionId}"]`).classList.add('active');
    
    currentSection = sectionId;
    currentPage = 1;
    
    loadSectionData(sectionId);
}

function loadSectionData(sectionId) {
    switch(sectionId) {
        case 'dashboard':
            updateDashboardStats();
            break;
        case 'users':
            populateUserSelect();
            populateUserRolesSelect();
            loadTableData('user', mockData.users);
            break;
        case 'persons':
            loadTableData('person', mockData.persons);
            break;
        case 'roles':
            loadFormPermissionsForRoles();
            loadTableData('role', mockData.roles);
            break;
        case 'permissions':
            loadTableData('permission', mockData.permissions);
            break;
        case 'forms':
            populateFormModulesSelect();
            loadTableData('form', mockData.forms);
            break;
        case 'modules':
            loadTableData('module', mockData.modules);
            break;
    }
}

function updateDashboardStats() {
    document.getElementById('user-count').textContent = mockData.users.length;
    document.getElementById('person-count').textContent = mockData.persons.length;
    document.getElementById('role-count').textContent = mockData.roles.length;
    document.getElementById('permission-count').textContent = mockData.permissions.length;
}

function loadTableData(entityType, data) {
    const tableBody = document.querySelector(`#${entityType}-table tbody`);
    tableBody.innerHTML = '';
    
    const filteredData = filterData(entityType, data);
    const paginatedData = paginateData(filteredData);
    
    if (paginatedData.length === 0) {
        const emptyRow = document.createElement('tr');
        emptyRow.innerHTML = `<td colspan="7">No hay datos disponibles</td>`;
        tableBody.appendChild(emptyRow);
    } else {
        paginatedData.forEach(item => {
            const row = document.createElement('tr');
            
            switch(entityType) {
                case 'user':
                    const person = mockData.persons.find(p => p.id === item.personId);
                    row.innerHTML = `
                        <td>${item.id}</td>
                        <td>${item.username}</td>
                        <td>${item.code}</td>
                        <td>${item.active ? 'Sí' : 'No'}</td>
                        <td>${person ? `${person.firstName} ${person.lastName}` : 'N/A'}</td>
                        <td>${item.created}</td>
                        <td>
                            <button class="edit-btn" data-id="${item.id}">Editar</button>
                            <button class="delete-btn" data-id="${item.id}">Eliminar</button>
                        </td>
                    `;
                    break;
                case 'person':
                    row.innerHTML = `
                        <td>${item.id}</td>
                        <td>${item.firstName}</td>
                        <td>${item.lastName}</td>
                        <td>${item.email}</td>
                        <td>
                            <button class="edit-btn" data-id="${item.id}">Editar</button>
                            <button class="delete-btn" data-id="${item.id}">Eliminar</button>
                        </td>
                    `;
                    break;
                case 'role':
                    row.innerHTML = `
                        <td>${item.id}</td>
                        <td>${item.name}</td>
                        <td>${item.description}</td>
                        <td>${item.active ? 'Sí' : 'No'}</td>
                        <td>${item.created}</td>
                        <td>
                            <button class="edit-btn" data-id="${item.id}">Editar</button>
                            <button class="delete-btn" data-id="${item.id}">Eliminar</button>
                        </td>
                    `;
                    break;
                case 'permission':
                    row.innerHTML = `
                        <td>${item.id}</td>
                        <td>${item.name}</td>
                        <td>${item.code}</td>
                        <td>${item.active ? 'Sí' : 'No'}</td>
                        <td>${item.created}</td>
                        <td>
                            <button class="edit-btn" data-id="${item.id}">Editar</button>
                            <button class="delete-btn" data-id="${item.id}">Eliminar</button>
                        </td>
                    `;
                    break;
                case 'form':
                    row.innerHTML = `
                        <td>${item.id}</td>
                        <td>${item.name}</td>
                        <td>${item.code}</td>
                        <td>${item.active ? 'Sí' : 'No'}</td>
                        <td>${item.created}</td>
                        <td>
                            <button class="edit-btn" data-id="${item.id}">Editar</button>
                            <button class="delete-btn" data-id="${item.id}">Eliminar</button>
                        </td>
                    `;
                    break;
                case 'module':
                    row.innerHTML = `
                        <td>${item.id}</td>
                        <td>${item.name}</td>
                        <td>${item.active ? 'Sí' : 'No'}</td>
                        <td>${item.created}</td>
                        <td>
                            <button class="edit-btn" data-id="${item.id}">Editar</button>
                            <button class="delete-btn" data-id="${item.id}">Eliminar</button>
                        </td>
                    `;
                    break;
            }
            
            tableBody.appendChild(row);
        });
    }
    
    setupTableEventListeners(entityType);
    updatePagination(entityType, filteredData.length);
}

function filterData(entityType, data) {
    const searchTerm = document.getElementById(`${entityType}-search`).value.toLowerCase();
    
    if (!searchTerm) return data;
    
    return data.filter(item => {
        switch(entityType) {
            case 'user':
                const person = mockData.persons.find(p => p.id === item.personId);
                return (
                    item.username.toLowerCase().includes(searchTerm) ||
                    item.code.toLowerCase().includes(searchTerm) ||
                    (person && `${person.firstName} ${person.lastName}`.toLowerCase().includes(searchTerm))
                );
            case 'person':
                return (
                    item.firstName.toLowerCase().includes(searchTerm) ||
                    item.lastName.toLowerCase().includes(searchTerm) ||
                    item.email.toLowerCase().includes(searchTerm)
                );
            case 'role':
                return (
                    item.name.toLowerCase().includes(searchTerm) ||
                    item.description.toLowerCase().includes(searchTerm)
                );
            case 'permission':
            case 'form':
                return (
                    item.name.toLowerCase().includes(searchTerm) ||
                    item.code.toLowerCase().includes(searchTerm)
                );
            case 'module':
                return item.name.toLowerCase().includes(searchTerm);
            default:
                return false;
        }
    });
}

function paginateData(data) {
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    return data.slice(startIndex, endIndex);
}

function updatePagination(entityType, totalItems) {
    const paginationContainer = document.getElementById(`${entityType}-pagination`);
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    
    paginationContainer.innerHTML = '';
    
    if (totalPages <= 1) return;
    
    const prevButton = document.createElement('button');
    prevButton.textContent = 'Anterior';
    prevButton.disabled = currentPage === 1;
    prevButton.addEventListener('click', () => {
        if (currentPage > 1) {
            currentPage--;
            loadTableData(entityType, mockData[`${entityType}s`]);
        }
    });
    paginationContainer.appendChild(prevButton);
    
    for (let i = 1; i <= totalPages; i++) {
        const pageButton = document.createElement('button');
        pageButton.textContent = i;
        pageButton.classList.toggle('active', i === currentPage);
        pageButton.addEventListener('click', () => {
            currentPage = i;
            loadTableData(entityType, mockData[`${entityType}s`]);
        });
        paginationContainer.appendChild(pageButton);
    }
    
    const nextButton = document.createElement('button');
    nextButton.textContent = 'Siguiente';
    nextButton.disabled = currentPage === totalPages;
    nextButton.addEventListener('click', () => {
        if (currentPage < totalPages) {
            currentPage++;
            loadTableData(entityType, mockData[`${entityType}s`]);
        }
    });
    paginationContainer.appendChild(nextButton);
}

function setupTableEventListeners(entityType) {
    // Configurar botones de edición
    document.querySelectorAll(`#${entityType}-table .edit-btn`).forEach(button => {
        button.addEventListener('click', () => {
            const id = parseInt(button.getAttribute('data-id'));
            editItem(entityType, id);
        });
    });
    
    // Configurar botones de eliminación
    document.querySelectorAll(`#${entityType}-table .delete-btn`).forEach(button => {
        button.addEventListener('click', () => {
            const id = parseInt(button.getAttribute('data-id'));
            deleteItem(entityType, id);
        });
    });
    
    // Configurar búsqueda
    document.getElementById(`${entityType}-search`).addEventListener('input', () => {
        currentPage = 1;
        loadTableData(entityType, mockData[`${entityType}s`]);
    });
}

function editItem(entityType, id) {
    const item = mockData[`${entityType}s`].find(item => item.id === id);
    
    if (!item) return;
    
    switch(entityType) {
        case 'user':
            document.getElementById('user-id').value = item.id;
            document.getElementById('user-username').value = item.username;
            document.getElementById('user-code').value = item.code;
            document.getElementById('user-active').checked = item.active;
            document.getElementById('user-person').value = item.personId;
            
            const userRolesSelect = document.getElementById('user-roles');
            for (let i = 0; i < userRolesSelect.options.length; i++) {
                userRolesSelect.options[i].selected = item.roles.includes(parseInt(userRolesSelect.options[i].value));
            }
            break;
        case 'person':
            document.getElementById('person-id').value = item.id;
            document.getElementById('person-firstname').value = item.firstName;
            document.getElementById('person-lastname').value = item.lastName;
            document.getElementById('person-email').value = item.email;
            break;
        case 'role':
            document.getElementById('role-id').value = item.id;
            document.getElementById('role-name').value = item.name;
            document.getElementById('role-description').value = item.description;
            document.getElementById('role-active').checked = item.active;
            
            // Marcar los permisos correspondientes
            document.querySelectorAll('#role-permissions-container input[type="checkbox"]').forEach(checkbox => {
                const [formId, permId] = checkbox.value.split('-').map(Number);
                checkbox.checked = item.formPermissions.some(fp => fp.formId === formId && fp.permissionId === permId);
            });
            break;
        case 'permission':
            document.getElementById('permission-id').value = item.id;
            document.getElementById('permission-name').value = item.name;
            document.getElementById('permission-code').value = item.code;
            document.getElementById('permission-active').checked = item.active;
            break;
        case 'form':
            document.getElementById('form-id').value = item.id;
            document.getElementById('form-name').value = item.name;
            document.getElementById('form-code').value = item.code;
            document.getElementById('form-active').checked = item.active;
            
            const formModulesSelect = document.getElementById('form-modules');
            for (let i = 0; i < formModulesSelect.options.length; i++) {
                formModulesSelect.options[i].selected = item.modules.includes(parseInt(formModulesSelect.options[i].value));
            }
            break;
        case 'module':
            document.getElementById('module-id').value = item.id;
            document.getElementById('module-name').value = item.name;
            document.getElementById('module-active').checked = item.active;
            break;
    }
}

function deleteItem(entityType, id) {
    displayModal(
        'Confirmar eliminación',
        `¿Estás seguro de que deseas eliminar este ${getSingularEntityName(entityType)}?`,
        () => {
            mockData[`${entityType}s`] = mockData[`${entityType}s`].filter(item => item.id !== id);
            loadTableData(entityType, mockData[`${entityType}s`]);
            updateDashboardStats();
        }
    );
}

function getSingularEntityName(entityType) {
    switch(entityType) {
        case 'user': return 'usuario';
        case 'person': return 'persona';
        case 'role': return 'rol';
        case 'permission': return 'permiso';
        case 'form': return 'formulario';
        case 'module': return 'módulo';
        default: return entityType;
    }
}

// Funciones específicas para cada sección
function populateUserSelect() {
    const personSelect = document.getElementById('user-person');
    personSelect.innerHTML = '<option value="">-- Seleccionar Persona --</option>';
    
    mockData.persons.forEach(person => {
        const option = document.createElement('option');
        option.value = person.id;
        option.textContent = `${person.firstName} ${person.lastName}`;
        personSelect.appendChild(option);
    });
}

function populateUserRolesSelect() {
    const rolesSelect = document.getElementById('user-roles');
    rolesSelect.innerHTML = '';
    
    mockData.roles.filter(role => role.active).forEach(role => {
        const option = document.createElement('option');
        option.value = role.id;
        option.textContent = role.name;
        rolesSelect.appendChild(option);
    });
}

function populateFormModulesSelect() {
    const modulesSelect = document.getElementById('form-modules');
    modulesSelect.innerHTML = '';
    
    mockData.modules.filter(module => module.active).forEach(module => {
        const option = document.createElement('option');
        option.value = module.id;
        option.textContent = module.name;
        modulesSelect.appendChild(option);
    });
}

function loadFormPermissionsForRoles() {
    const container = document.getElementById('role-permissions-container');
    container.innerHTML = '';
    
    mockData.forms.filter(form => form.active).forEach(form => {
        const formDiv = document.createElement('div');
        formDiv.className = 'permission-form-container';
        
        const formHeader = document.createElement('h5');
        formHeader.textContent = form.name;
        formDiv.appendChild(formHeader);
        
        const permissionsDiv = document.createElement('div');
        permissionsDiv.className = 'permissions-group';
        
        mockData.permissions.filter(perm => perm.active).forEach(perm => {
            const checkboxId = `perm-${form.id}-${perm.id}`;
            
            const checkboxContainer = document.createElement('div');
            checkboxContainer.className = 'permission-checkbox';
            
            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.id = checkboxId;
            checkbox.value = `${form.id}-${perm.id}`;
            
            const label = document.createElement('label');
            label.htmlFor = checkboxId;
            label.textContent = perm.name;
            
            checkboxContainer.appendChild(checkbox);
            checkboxContainer.appendChild(label);
            permissionsDiv.appendChild(checkboxContainer);
        });
        
        formDiv.appendChild(permissionsDiv);
        container.appendChild(formDiv);
    });
}

// Manejadores de eventos para formularios
function setupFormEventListeners() {
    // Formulario de usuarios
    document.getElementById('user-form').addEventListener('submit', function(e) {
        e.preventDefault();
        saveUser();
    });
    document.getElementById('user-cancel').addEventListener('click', function() {
        resetForm('user');
    });
    
    // Formulario de personas
    document.getElementById('person-form').addEventListener('submit', function(e) {
        e.preventDefault();
        savePerson();
    });
    document.getElementById('person-cancel').addEventListener('click', function() {
        resetForm('person');
    });
    
    // Formulario de roles
    document.getElementById('role-form').addEventListener('submit', function(e) {
        e.preventDefault();
        saveRole();
    });
    document.getElementById('role-cancel').addEventListener('click', function() {
        resetForm('role');
    });
    
    // Formulario de permisos
    document.getElementById('permission-form').addEventListener('submit', function(e) {
        e.preventDefault();
        savePermission();
    });
    document.getElementById('permission-cancel').addEventListener('click', function() {
        resetForm('permission');
    });
    
    // Formulario de formularios
    document.getElementById('form-form').addEventListener('submit', function(e) {
        e.preventDefault();
        saveForm();
    });
    document.getElementById('form-cancel').addEventListener('click', function() {
        resetForm('form');
    });
    
    // Formulario de módulos
    document.getElementById('module-form').addEventListener('submit', function(e) {
        e.preventDefault();
        saveModule();
    });
    document.getElementById('module-cancel').addEventListener('click', function() {
        resetForm('module');
    });
}

function resetForm(entityType) {
    document.getElementById(`${entityType}-form`).reset();
    document.getElementById(`${entityType}-id`).value = '';
}

function saveUser() {
    const id = document.getElementById('user-id').value;
    const username = document.getElementById('user-username').value;
    const code = document.getElementById('user-code').value;
    const active = document.getElementById('user-active').checked;
    const personId = parseInt(document.getElementById('user-person').value);
    
    const rolesSelect = document.getElementById('user-roles');
    const roles = Array.from(rolesSelect.selectedOptions).map(option => parseInt(option.value));
    
    if (!username || !code || !personId) {
        alert('Por favor, complete todos los campos obligatorios.');
        return;
    }
    
    if (id) {
        // Actualizar usuario existente
        const userIndex = mockData.users.findIndex(user => user.id === parseInt(id));
        if (userIndex !== -1) {
            mockData.users[userIndex] = {
                ...mockData.users[userIndex],
                username,
                code,
                active,
                personId,
                roles
            };
        }
    } else {
        // Crear nuevo usuario
        const newId = mockData.users.length > 0 ? Math.max(...mockData.users.map(u => u.id)) + 1 : 1;
        mockData.users.push({
            id: newId,
            username,
            code,
            active,
            personId,
            roles,
            created: new Date().toISOString().split('T')[0]
        });
    }
    
    resetForm('user');
    loadTableData('user', mockData.users);
    updateDashboardStats();
}

function savePerson() {
    const id = document.getElementById('person-id').value;
    const firstName = document.getElementById('person-firstname').value;
    const lastName = document.getElementById('person-lastname').value;
    const email = document.getElementById('person-email').value;
    
    if (!firstName || !lastName || !email) {
        alert('Por favor, complete todos los campos obligatorios.');
        return;
    }
    
    if (id) {
        // Actualizar persona existente
        const personIndex = mockData.persons.findIndex(person => person.id === parseInt(id));
        if (personIndex !== -1) {
            mockData.persons[personIndex] = {
                ...mockData.persons[personIndex],
                firstName,
                lastName,
                email
            };
        }
    } else {
        // Crear nueva persona
        const newId = mockData.persons.length > 0 ? Math.max(...mockData.persons.map(p => p.id)) + 1 : 1;
        mockData.persons.push({
            id: newId,
            firstName,
            lastName,
            email
        });
    }
    
    resetForm('person');
    loadTableData('person', mockData.persons);
    updateDashboardStats();
}

function saveRole() {
    const id = document.getElementById('role-id').value;
    const name = document.getElementById('role-name').value;
    const description = document.getElementById('role-description').value;
    const active = document.getElementById('role-active').checked;
    
    // Recopilar permisos seleccionados
    const formPermissions = [];
    document.querySelectorAll('#role-permissions-container input[type="checkbox"]:checked').forEach(checkbox => {
        const [formId, permId] = checkbox.value.split('-').map(Number);
        formPermissions.push({
            formId,
            permissionId: permId
        });
    });
    
    if (!name || !description) {
        alert('Por favor, complete todos los campos obligatorios.');
        return;
    }
    
    if (id) {
        // Actualizar rol existente
        const roleIndex = mockData.roles.findIndex(role => role.id === parseInt(id));
        if (roleIndex !== -1) {
            mockData.roles[roleIndex] = {
                ...mockData.roles[roleIndex],
                name,
                description,
                active,
                formPermissions
            };
        }
    } else {
        // Crear nuevo rol
        const newId = mockData.roles.length > 0 ? Math.max(...mockData.roles.map(r => r.id)) + 1 : 1;
        mockData.roles.push({
            id: newId,
            name,
            description,
            active,
            formPermissions,
            created: new Date().toISOString().split('T')[0]
        });
    }
    
    resetForm('role');
    loadTableData('role', mockData.roles);
    updateDashboardStats();
}

function savePermission() {
    const id = document.getElementById('permission-id').value;
    const name = document.getElementById('permission-name').value;
    const code = document.getElementById('permission-code').value;
    const active = document.getElementById('permission-active').checked;
    
    if (!name || !code) {
        alert('Por favor, complete todos los campos obligatorios.');
        return;
    }
    
    if (id) {
        // Actualizar permiso existente
        const permIndex = mockData.permissions.findIndex(perm => perm.id === parseInt(id));
        if (permIndex !== -1) {
            mockData.permissions[permIndex] = {
                ...mockData.permissions[permIndex],
                name,
                code,
                active
            };
        }
    } else {
        // Crear nuevo permiso
        const newId = mockData.permissions.length > 0 ? Math.max(...mockData.permissions.map(p => p.id)) + 1 : 1;
        mockData.permissions.push({
            id: newId,
            name,
            code,
            active,
            created: new Date().toISOString().split('T')[0]
        });
    }
    
    resetForm('permission');
    loadFormPermissionsForRoles(); // Actualizar permisos en el formulario de roles
    loadTableData('permission', mockData.permissions);
    updateDashboardStats();
}

function saveForm() {
    const id = document.getElementById('form-id').value;
    const name = document.getElementById('form-name').value;
    const code = document.getElementById('form-code').value;
    const active = document.getElementById('form-active').checked;
    
    const modulesSelect = document.getElementById('form-modules');
    const modules = Array.from(modulesSelect.selectedOptions).map(option => parseInt(option.value));
    
    if (!name || !code) {
        alert('Por favor, complete todos los campos obligatorios.');
        return;
    }
    
    if (id) {
        // Actualizar formulario existente
        const formIndex = mockData.forms.findIndex(form => form.id === parseInt(id));
        if (formIndex !== -1) {
            mockData.forms[formIndex] = {
                ...mockData.forms[formIndex],
                name,
                code,
                active,
                modules
            };
        }
    } else {
        // Crear nuevo formulario
        const newId = mockData.forms.length > 0 ? Math.max(...mockData.forms.map(f => f.id)) + 1 : 1;
        mockData.forms.push({
            id: newId,
            name,
            code,
            active,
            modules,
            created: new Date().toISOString().split('T')[0]
        });
    }
    
    resetForm('form');
    loadFormPermissionsForRoles(); // Actualizar formularios en el panel de roles
    loadTableData('form', mockData.forms);
    updateDashboardStats();
}

function saveModule() {
    const id = document.getElementById('module-id').value;
    const name = document.getElementById('module-name').value;
    const active = document.getElementById('module-active').checked;
    
    if (!name) {
        alert('Por favor, complete todos los campos obligatorios.');
        return;
    }
    
    if (id) {
        // Actualizar módulo existente
        const moduleIndex = mockData.modules.findIndex(module => module.id === parseInt(id));
        if (moduleIndex !== -1) {
            mockData.modules[moduleIndex] = {
                ...mockData.modules[moduleIndex],
                name,
                active
            };
        }
    } else {
        // Crear nuevo módulo
        const newId = mockData.modules.length > 0 ? Math.max(...mockData.modules.map(m => m.id)) + 1 : 1;
        mockData.modules.push({
            id: newId,
            name,
            active,
            created: new Date().toISOString().split('T')[0]
        });
    }
    
    resetForm('module');
    populateFormModulesSelect(); // Actualizar módulos en el formulario de forms
    loadTableData('module', mockData.modules);
    updateDashboardStats();
}

// Configuración de la navegación
function setupNavigation() {
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const sectionId = link.getAttribute('data-section');
            showSection(sectionId);
        });
    });
}

// Inicialización
document.addEventListener('DOMContentLoaded', function() {
    // Inicializar datos de ejemplo
    initializeMockData();
    
    // Configurar listeners de navegación
    setupNavigation();
    
    // Configurar listeners de formularios
    setupFormEventListeners();
    
    // Mostrar la sección inicial (dashboard)
    showSection('dashboard');
    
    // Inicializar la búsqueda en todas las tablas
    ['user', 'person', 'role', 'permission', 'form', 'module'].forEach(entityType => {
        const searchInput = document.getElementById(`${entityType}-search`);
        if (searchInput) {
            searchInput.value = '';
        }
    });
});