/// <binding />
module.exports = function (grunt) {
    require('swagger-js-codegen');
    grunt.initConfig({
        'swagger-js-codegen': {
            queries: {
                options: {
                    apis: [
                        {
                            swagger: 'http://localhost:62614/swagger/docs/v1',
                            moduleName: 'GameProxy',
                            fileName: 'GameProxy.js',
                            className: 'GameService',
                            mustache: {
                                moduleName: 'GameProxy'
                            },
                            template: {
                                class: 'swagger/templates/angular-class.mustache',
                                method: 'swagger/templates/method.mustache',
                                request: 'swagger/templates/angular-request.mustache'
                            },
                            custom: true
                        }
                    ],
                    dest: 'swagger'
                },
                dist: {
                }
            }
        }
    });

    grunt.loadNpmTasks('grunt-swagger-js-codegen');

    grunt.task.registerTask('default', ['swagger-js-codegen']);
};