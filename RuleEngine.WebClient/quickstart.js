Ext.require(['*']);

Ext.onReady(function () {
    var store = Ext.create('Ext.data.TreeStore', {
        proxy: {
            type: 'ajax',
            url: 'data/rulesettree.json'
        },
        root: {
            text: 'RuleSets',
            id: 'src',
            expanded: true
        },
        folderSort: true,
        sorters: [{
            property: 'text',
            direction: 'ASC'
        }]
    });

    var tree = Ext.create('Ext.tree.Panel', {
        renderTo: Ext.getBody(),
        id: 'tree',
        width: 250,
        height: 300,
        useArrows: true,
        rootVisible: false,
        lines: false,
        store: store,
        listeners: {
            selectionchange: function (model, records) {
                if (records[0]) {
                    //this.up('form').getForm().loadRecord(records[0]);
                }
            }
        }
    });

});
